using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Mathematics;

using TransportX.Diagnostics;
using TransportX.Rendering;
using TransportX.Rendering.Pipelines;
using TransportX.Spatial;

using TransportX.Scripting;

using TransportX.Domains.Equipment.Cameras;

namespace TransportX.Domains.Equipment.Scripting.Commands
{
    public abstract class SceneCaptureCameraFactoryBase<T> where T : SceneCaptureCameraFactoryBase<T>
    {
        private readonly CamerasBase Parent;
        private readonly ConcurrentDictionary<TransformedModel, HashSet<Material>> Projections = [];

        private IWorldObject? AttachedTo = null;
        private SizeI TextureSizeValue = new(64, 64);
        private float FieldOfViewValue = float.Pi / 2;
        private float AspectRatioValue = 1;
        private RenderPassFlags RenderFlags = RenderPassFlags.None;

        public string Key { get; }

        public SceneCaptureCameraCommand? BuiltCamera { get; private set; } = null;

        private protected SceneCaptureCameraFactoryBase(CamerasBase parent, string key)
        {
            Parent = parent;
            Key = key;
        }

        public T Position(IWorldObject attachTo)
        {
            AttachedTo = attachTo;
            return (T)this;
        }

        public T TextureSize(int width, int height)
        {
            TextureSizeValue = new SizeI(width, height);
            return (T)this;
        }

        public T FieldOfView(double degrees)
        {
            FieldOfViewValue = MathHelper.ToRadians((float)degrees);
            return (T)this;
        }

        public T AspectRatio(double ratio)
        {
            AspectRatioValue = (float)ratio;
            return (T)this;
        }

        public T Reflect()
        {
            RenderFlags |= RenderPassFlags.Reflect;
            return (T)this;
        }

        public T DisableShadows()
        {
            RenderFlags |= RenderPassFlags.DisableShadows;
            return (T)this;
        }

        public T ProjectOnto(TransformedModel model, Material targetMaterial)
        {
            if (!AsMeshModel(model.Resource.Model, out _)) return (T)this;

            HashSet<Material> targetMaterials = Projections.GetOrAdd(model, _ => []);
            targetMaterials.Add(targetMaterial);

            return (T)this;
        }

        public T ProjectOnto(TransformedModel model, string targetMaterialName)
        {
            if (!AsMeshModel(model.Resource.Model, out IMeshModel? meshModel)) return (T)this;

            Material? targetMaterial = meshModel.FindMaterial(targetMaterialName);
            if (targetMaterial is null)
            {
                ScriptError error = new(ErrorLevel.Error, $"材質 '{targetMaterialName}' が見つかりません。");
                Parent.ErrorCollector.Report(error);
                return (T)this;
            }

            return ProjectOnto(model, targetMaterial);
        }

        protected bool AsMeshModel(IModel model, [MaybeNullWhen(false)] out IMeshModel meshModel)
        {
            meshModel = model as IMeshModel;
            if (meshModel is null)
            {
                ScriptError error = new(ErrorLevel.Error, $"非対応のモデルが指定されました。モデルが {nameof(IMeshModel)} を実装している必要があります。");
                Parent.ErrorCollector.Report(error);
                return false;
            }

            return true;
        }

        public SceneCaptureCameraCommand Build()
        {
            if (BuiltCamera is not null)
            {
                ScriptError error = new(ErrorLevel.Error, "このカメラは既にビルド済です。");
                Parent.ErrorCollector.Report(error);
                return BuiltCamera;
            }

            if (AttachedTo is null) return ReportAndCreateEmpty("カメラの基準位置が指定されていません。");

            SceneCaptureCamera camera = new(Parent.GraphicsHost.Device, AttachedTo, TextureSizeValue)
            {
                FieldOfView = FieldOfViewValue,
                AspectRatio = AspectRatioValue,
                RenderFlags = RenderFlags,
            };
            BuiltCamera = new SceneCaptureCameraCommand(Parent.ErrorCollector, Key, camera);
            Parent.AddSceneCapture(BuiltCamera);

            foreach ((TransformedModel model, HashSet<Material> targetMaterials) in Projections)
            {
                Dictionary<Material, Material> materialOverrides = targetMaterials.ToDictionary(
                    targetMaterial => targetMaterial,
                    targetMaterial => ScreenMaterialFactory.Create(targetMaterial, camera.RenderTarget.ShaderResourceView));

                model.Resource = model.Resource with
                {
                    Model = new MaterialOverriddenModel((IMeshModel)model.Resource.Model, materialOverrides),
                };
            }

            return BuiltCamera;


            SceneCaptureCameraCommand ReportAndCreateEmpty(string message)
            {
                ScriptError error = new(ErrorLevel.Error, message);
                Parent.ErrorCollector.Report(error);
                return SceneCaptureCameraCommand.Empty(Parent.ErrorCollector, Key);
            }
        }
    }
}
