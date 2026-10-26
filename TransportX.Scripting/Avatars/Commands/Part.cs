using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

using TransportX.Components;
using TransportX.Diagnostics;
using TransportX.Rendering;
using TransportX.Spatial;

namespace TransportX.Scripting.Avatars.Commands
{
    public class Part
    {
        private readonly ScriptAvatar Avatar;
        private readonly IErrorCollector TextureErrorCollector;

        public string Key { get; }
        public TransformedModel Model { get; }

        public IComponentCollection<IComponent> Components { get; } = new ComponentCollection<IComponent>();

        internal Part(ScriptAvatar avatar, string key, TransformedModel model)
        {
            Avatar = avatar;

            TextureErrorCollector = IErrorCollector.Default();
            TextureErrorCollector.Reported += (sender, e) =>
            {
                ScriptError error = ScriptError.CreateFrom(e.Error);
                Avatar.ErrorCollector.Report(error);
            };

            Key = key;
            Model = model;
        }

        public static Part Empty(ScriptAvatar avatar, string key)
        {
            TransformedModel model = new(ModelResourceSet.Empty(), Pose.Identity);
            return new Part(avatar, key, model);
        }

        internal void RegisterComponents()
        {
            Avatar.ComponentEngine.Register(Components);
        }

        private bool AsMeshModel([MaybeNullWhen(false)] out IMeshModel meshModel)
        {
            if (Model.Resource.Model is IMeshModel cast)
            {
                meshModel = cast;
                return true;
            }
            else
            {
                ScriptError error = new(ErrorLevel.Error, $"パーツ '{Key}' のモデルは {nameof(IMeshModel)} ではないため、材質を上書きできません。");
                Avatar.ErrorCollector.Report(error);
                meshModel = null;
                return false;
            }
        }

        public Part OverrideMaterial(Material targetMaterial, Action<Material> overrider)
        {
            if (!AsMeshModel(out IMeshModel? baseModel)) return this;

            Material material = targetMaterial.Clone();
            overrider(material);

            MaterialOverriddenModel overridden = new(baseModel, new Dictionary<Material, Material>()
            {
                { targetMaterial, material },
            });

            Model.Resource = Model.Resource with
            {
                Model = overridden,
            };
            return this;
        }

        public Part OverrideMaterial(string targetMaterialName, Action<Material> overrider)
        {
            if (!AsMeshModel(out IMeshModel? baseModel)) return this;

            Material? targetMaterial = baseModel.FindMaterial(targetMaterialName);
            if (targetMaterial is null)
            {
                ScriptError error = new(ErrorLevel.Error, $"パーツ '{Key}' のモデルに材質 '{targetMaterialName}' が見つかりません。");
                Avatar.ErrorCollector.Report(error);
                return this;
            }

            return OverrideMaterial(targetMaterial, overrider);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private ID3D11ShaderResourceView? LoadTexture(string filePath, bool isLinear)
        {
            try
            {
                string absolutePath = Path.Combine(BaseDirectory.Find(3) ?? Avatar.BaseDirectory, filePath);
                if (!File.Exists(absolutePath)) throw new FileNotFoundException(null, absolutePath);

                TextureFactory textureFactory = Avatar.Commander.Structure.Parts.TextureFactory;
                ID3D11ShaderResourceView shaderResourceView = textureFactory.CreateFromFile(absolutePath, isLinear, TextureErrorCollector);

                Avatar.Commander.Structure.Parts.CreatedTextures.Add(shaderResourceView);
                return shaderResourceView;
            }
            catch (Exception ex)
            {
                string? message = ex is FileNotFoundException fex ? $"テクスチャファイル '{fex.FileName}' が見つかりませんでした。" : null;
                ScriptError error = new(ErrorLevel.Error, ex, message);
                Avatar.ErrorCollector.Report(error);
                return null;
            }
        }

        public Part OverrideBaseColorTexture(string targetMaterialName, ID3D11ShaderResourceView texture)
        {
            TextureSlot slot = new(texture, TextureSampleMode.Texture);
            return OverrideMaterial(targetMaterialName, material => material.BaseColorTexture = slot);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Part OverrideBaseColorTexture(string targetMaterialName, string texturePath)
        {
            ID3D11ShaderResourceView? shaderResourceView = LoadTexture(texturePath, false);
            if (shaderResourceView is null) return this;

            TextureSlot slot = new(shaderResourceView, TextureSampleMode.Texture);
            return OverrideMaterial(targetMaterialName, material => material.BaseColorTexture = slot);
        }

        public Part OverrideEmissiveTexture(string targetMaterialName, ID3D11ShaderResourceView shaderResourceView)
        {
            TextureSlot slot = new(shaderResourceView, TextureSampleMode.Texture);
            return OverrideMaterial(targetMaterialName, material => material.EmissiveTexture = slot);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Part OverrideEmissiveTexture(string targetMaterialName, string texturePath)
        {
            ID3D11ShaderResourceView? shaderResourceView = LoadTexture(texturePath, false);
            if (shaderResourceView is null) return this;

            TextureSlot slot = new(shaderResourceView, TextureSampleMode.Texture);
            return OverrideMaterial(targetMaterialName, material => material.EmissiveTexture = slot);
        }
    }

    public class KinematicPart : Part
    {
        public new KinematicTransformedModel Model { get; }

        internal KinematicPart(ScriptAvatar avatar, string key, KinematicTransformedModel model) : base(avatar, key, model)
        {
            Model = model;
        }

        public static KinematicPart InvalidEmpty(ScriptAvatar avatar, string key)
        {
            return new KinematicPart(avatar, key, null!);
        }
    }

    public class DynamicPart : Part
    {
        public new DynamicTransformedModel Model { get; }

        internal DynamicPart(ScriptAvatar avatar, string key, DynamicTransformedModel model) : base(avatar, key, model)
        {
            Model = model;
        }

        public static DynamicPart InvalidEmpty(ScriptAvatar avatar, string key)
        {
            return new DynamicPart(avatar, key, null!);
        }
    }
}
