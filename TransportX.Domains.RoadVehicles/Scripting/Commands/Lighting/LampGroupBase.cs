using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;
using TransportX.Rendering;
using TransportX.Spatial;

using TransportX.Scripting;
using TransportX.Scripting.Avatars.Commands;

using TransportX.Domains.RoadVehicles.Lighting;

namespace TransportX.Domains.RoadVehicles.Scripting.Commands.Lighting
{
    public abstract class LampGroupBase<T>
    {
        protected readonly Lighting Parent;

        protected LampGroupBase(Lighting parent)
        {
            Parent = parent;
        }

        protected abstract T CreateEmptyLamp(string key);
        public abstract T AddLamp(string key, ILamp lamp);

        public T AddVisibilityLamp(string key, TransformedModel model)
            => AddLamp(key, new VisibilityLamp(model));

        public T AddVisibilityLamp(string key, string partKey)
        {
            return Parent.Avatar.Commander.Structure.Parts.All.GetValue(partKey, out Part part)
                ? AddVisibilityLamp(key, part.Model)
                : CreateEmptyLamp(key);
        }

        public T AddEmissiveLamp(string key, TransformedModel model, string materialName, Vector3 emissive, double timeConstant)
        {
            if (!AsMeshModel(model.Resource.Model, out IMeshModel? baseModel)) return CreateEmptyLamp(key);

            Material? targetMaterial = baseModel.FindMaterial(materialName);
            if (targetMaterial is null)
            {
                ScriptError error = new(ErrorLevel.Error, $"材質 '{materialName}' がモデル内に見つかりません。");
                Parent.Avatar.ErrorCollector.Report(error);
                return CreateEmptyLamp(key);
            }

            Material newMaterial = targetMaterial.Clone();
            Dictionary<Material, Material> materialOverrides = new()
            {
                { targetMaterial, newMaterial },
            };
            MaterialOverriddenModel newModel = new(baseModel, materialOverrides);

            model.Resource = model.Resource with
            {
                Model = newModel,
            };
            EmissiveLamp lamp = new(newMaterial, emissive, (float)timeConstant);

            return AddLamp(key, lamp);
        }

        public T AddEmissiveLamp(string key, TransformedModel model, string materialName, double r, double g, double b, double timeConstant)
        {
            Vector3 activeEmissive = new((float)r, (float)g, (float)b);
            return AddEmissiveLamp(key, model, materialName, activeEmissive, timeConstant);
        }

        public T AddEmissiveLamp(string key, string partKey, string materialName, double r, double g, double b, double timeConstant)
        {
            return Parent.Avatar.Commander.Structure.Parts.All.GetValue(partKey, out Part part)
                ? AddEmissiveLamp(key, part.Model, materialName, r, g, b, timeConstant)
                : CreateEmptyLamp(key);
        }

        private bool AsMeshModel(IModel model, [MaybeNullWhen(false)] out IMeshModel meshModel)
        {
            meshModel = model as IMeshModel;
            if (meshModel is null)
            {
                ScriptError error = new(ErrorLevel.Error, $"非対応のモデルが指定されました。モデルが {nameof(IMeshModel)} を実装している必要があります。");
                Parent.Avatar.ErrorCollector.Report(error);
                return false;
            }

            return true;
        }
    }
}
