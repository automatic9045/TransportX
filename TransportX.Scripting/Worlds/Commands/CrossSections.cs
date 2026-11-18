using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics.Constraints;
using Vortice.Direct3D11;

using TransportX.Data;
using TransportX.Diagnostics;
using TransportX.Physics;
using TransportX.Rendering;
using TransportX.Rendering.Procedural;
using TransportX.Extensions.Utilities;
using TransportX.Scripting.Collections;

namespace TransportX.Scripting.Worlds.Commands
{
    public class CrossSections
    {
        private readonly ScriptWorld World;

        private readonly TextureFactory TextureFactory;
        private readonly List<ID3D11ShaderResourceView> CreatedTextures = [];

        private readonly ScriptDictionary<string, CrossSection> AllKey;
        public IReadOnlyScriptDictionary<string, CrossSection> All => AllKey;

        internal CrossSections(ScriptWorld world)
        {
            World = world;

            TextureFactory = new TextureFactory(World.GraphicsHost.Context, World.GraphicsHost.WICFactory);
            AllKey = new ScriptDictionary<string, CrossSection>(World.ErrorCollector, "断面", _ => CrossSection.Empty);
        }

        internal void Dispose()
        {
            foreach (ID3D11ShaderResourceView shaderResourceView in CreatedTextures)
            {
                shaderResourceView.Dispose();
            }
        }

        public void Add(string key, CrossSection item)
        {
            AllKey[key] = item;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public CrossSection Load(string key, string path)
        {
            string fullPath = Path.GetFullPath(Path.Combine(BaseDirectory.Find() ?? World.BaseDirectory, path));
            if (!File.Exists(fullPath))
            {
                ScriptError error = new(ErrorLevel.Error, $"断面定義ファイル '{fullPath}' が見つかりません。");
                World.ErrorCollector.Report(error);
                Add(key, CrossSection.Empty);
                return CrossSection.Empty;
            }

            Data.Rendering.CrossSection? data = XmlSerializer<Data.Rendering.CrossSection>.FromXml(fullPath, World.ErrorCollector);
            if (data is null)
            {
                Add(key, CrossSection.Empty);
                return CrossSection.Empty;
            }

            World.ErrorCollector.ReportRange(data.Errors);
            string xmlDirectory = Path.GetDirectoryName(fullPath)!;

            Dictionary<string, Data.Rendering.CrossSectionVertex> vertexMap = [];
            for (int i = 0; i < data.Vertices.Count; i++)
            {
                Data.Rendering.CrossSectionVertex vertexData = data.Vertices[i];
                if (vertexData.Key.Value is null) continue;

                if (!vertexMap.TryAdd(vertexData.Key.Value, vertexData))
                {
                    Error error = vertexData.Key.CreateError($"重複した頂点キー '{vertexData.Key.Value}' が定義されています。");
                    World.ErrorCollector.Report(error);
                }
            }

            Dictionary<string, Material> materialMap = [];
            for (int i = 0; i < data.Materials.Count; i++)
            {
                Data.Rendering.Material materialData = data.Materials[i];
                if (materialData.Name.Value is null) continue;

                IErrorCollector textureErrorCollector = IErrorCollector.Default();
                textureErrorCollector.Reported += (sender, e) =>
                {
                    Error error = e.Error.Location is null
                        ? e.Error.ChangeSource(materialData.Location, materialData.LineNumber, materialData.LinePosition) : e.Error;
                    World.ErrorCollector.Report(error);
                };

                TextureSlot ormTexture = TextureSlot.Empty;
                try
                {
                    string? occlusionTexturePath = GetTextureFullPath(materialData.OcclusionTexture);
                    string? roughnessTexturePath = GetTextureFullPath(materialData.RoughnessTexture);
                    string? metallicTexturePath = GetTextureFullPath(materialData.MetallicTexture);

                    ID3D11ShaderResourceView? shaderResourceView = TextureFactory.CreateMergedFromFiles(
                        occlusionTexturePath, roughnessTexturePath, metallicTexturePath, true, textureErrorCollector);

                    if (shaderResourceView is not null)
                    {
                        CreatedTextures.Add(shaderResourceView);
                        ormTexture = new(shaderResourceView, TextureSampleMode.Texture);
                    }


                    string? GetTextureFullPath(Data.Rendering.TextureSlot slot)
                        => slot.Path.Value is null ? null : Path.GetFullPath(Path.Combine(xmlDirectory, slot.Path.Value));
                }
                catch (IOException ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
                {
                    string missingPath = ex is FileNotFoundException fex && fex.FileName is not null ? fex.FileName
                        : $"{materialData.OcclusionTexture.Path}, {materialData.RoughnessTexture.Path}, {materialData.MetallicTexture.Path}";
                    Error error = materialData.Name.CreateError($"テクスチャファイル '{missingPath}' が見つかりませんでした。", exception: ex);
                    World.ErrorCollector.Report(error);
                }
                catch (Exception ex)
                {
                    Error error = materialData.Name.CreateError(
                        $"PBR 遮蔽テクスチャ '{materialData.OcclusionTexture.Path}'、粗さテクスチャ '{materialData.RoughnessTexture.Path}'、" +
                        $"金属感テクスチャ '{materialData.MetallicTexture.Path}' の合成に失敗しました。", exception: ex);
                    World.ErrorCollector.Report(error);
                }

                Material material = new()
                {
                    Name = materialData.Name.Value,
                    BaseColor = materialData.BaseColor.Value.ToVector4(),
                    Metallic = materialData.Metallic.Value,
                    Roughness = materialData.Roughness.Value,
                    Emissive = materialData.Emissive.Value.ToVector3() * materialData.EmissiveStrength.Value,
                    BaseColorTexture = LoadTextureSlot(materialData.BaseColorTexture, false),
                    NormalTexture = LoadTextureSlot(materialData.NormalTexture, true),
                    ORMTexture = ormTexture,
                    EmissiveTexture = LoadTextureSlot(materialData.EmissiveTexture, false),
                };

                if (!materialMap.TryAdd(materialData.Name.Value, material))
                {
                    Error error = materialData.Name.CreateError($"重複した材質名 '{materialData.Name.Value}' が定義されています。");
                    World.ErrorCollector.Report(error);
                }
            }

            Dictionary<string, ColliderMaterial> colliderMaterialMap = [];
            for (int i = 0; i < data.ColliderMaterials.Count; i++)
            {
                Data.Rendering.ColliderMaterial materialData = data.ColliderMaterials[i];
                if (materialData.Name.Value is null) continue;

                SpringSettings springSettings = new(materialData.SpringFrequency.Value, materialData.SpringDampingRatio.Value);
                ColliderMaterial colliderMaterial = new(materialData.FrictionCoefficient.Value, materialData.MaxRecoveryVelocity.Value, springSettings);

                if (!colliderMaterialMap.TryAdd(materialData.Name.Value, colliderMaterial))
                {
                    Error error = materialData.Name.CreateError($"重複した物理材質名 '{materialData.Name.Value}' が定義されています。");
                    World.ErrorCollector.Report(error);
                }
            }

            List<CrossSectionSurface> surfaces = [];
            for (int i = 0; i < data.Surfaces.Count; i++)
            {
                Data.Rendering.CrossSectionSurface surfaceData = data.Surfaces[i];

                if (surfaceData.Material.Value is null) continue;
                if (!materialMap.TryGetValue(surfaceData.Material.Value, out Material? material))
                {
                    Error error = surfaceData.Material.CreateError($"材質 '{surfaceData.Material.Value}' が見つかりません。");
                    World.ErrorCollector.Report(error);
                    continue;
                }

                ColliderMaterial? colliderMaterial = null;
                if (surfaceData.ColliderMaterial.Value is not null)
                {
                    if (!colliderMaterialMap.TryGetValue(surfaceData.ColliderMaterial.Value, out ColliderMaterial resolvedColliderMaterial))
                    {
                        Error error = surfaceData.ColliderMaterial.CreateError($"物理材質 '{surfaceData.ColliderMaterial.Value}' が見つかりません。");
                        World.ErrorCollector.Report(error);
                        continue;
                    }
                    colliderMaterial = resolvedColliderMaterial;
                }

                List<CrossSectionVertex> surfaceVertices = [];
                for (int j = 0; j < surfaceData.Vertices.Count; j++)
                {
                    Data.Rendering.CrossSectionVertexReference vertexRef = surfaceData.Vertices[j];
                    if (vertexRef.Key.Value is null) continue;

                    if (!vertexMap.TryGetValue(vertexRef.Key.Value, out Data.Rendering.CrossSectionVertex? vertexDef))
                    {
                        Error error = vertexRef.Key.CreateError($"頂点 '{vertexRef.Key.Value}' が見つかりません。");
                        World.ErrorCollector.Report(error);
                        continue;
                    }

                    Vector2 position = new(vertexDef.X.Value, vertexDef.Y.Value);
                    Vector2 rawNormal = new(vertexDef.NormalX.Value, vertexDef.NormalY.Value);
                    Vector2 normal = 1e-6f < rawNormal.LengthSquared() ? Vector2.Normalize(rawNormal) : Vector2.UnitY;
                    float u = vertexDef.U.Value;
                    Vector4 color = vertexDef.Color.Value.ToVector4();

                    CrossSectionVertex vertex = new(position, normal, u)
                    {
                        Color = color,
                    };
                    surfaceVertices.Add(vertex);
                }

                if (surfaceVertices.Count < 2)
                {
                    Error error = surfaceData.CreateError("サーフェスには最低 2 つの有効な頂点が必要です。");
                    World.ErrorCollector.Report(error);
                    continue;
                }

                CrossSectionSurface surface = new(surfaceVertices, surfaceData.VInterval.Value, surfaceData.VOffset.Value, material, colliderMaterial);
                surfaces.Add(surface);
            }

            CrossSection crossSection = new(surfaces);
            Add(key, crossSection);
            return crossSection;


            TextureSlot LoadTextureSlot(Data.Rendering.TextureSlot slotData, bool isLinear)
            {
                if (slotData == Data.Rendering.TextureSlot.Empty || slotData.Path.Value is null) return TextureSlot.Empty;

                string texturePath = Path.GetFullPath(Path.Combine(xmlDirectory, slotData.Path.Value));
                try
                {
                    ID3D11ShaderResourceView shaderResourceView = TextureFactory.CreateFromFile(texturePath, isLinear, World.ErrorCollector);
                    CreatedTextures.Add(shaderResourceView);
                    return new TextureSlot(shaderResourceView, slotData.SampleMode.Value);
                }
                catch (FileNotFoundException ex)
                {
                    Error error = slotData.Path.CreateError($"テクスチャファイル '{texturePath}' が見つかりませんでした。", exception: ex);
                    World.ErrorCollector.Report(error);
                    return TextureSlot.Empty;
                }
                catch (Exception ex)
                {
                    Error error = slotData.Path.CreateError($"テクスチャファイル '{texturePath}' を読み込めませんでした。", exception: ex);
                    World.ErrorCollector.Report(error);
                    return TextureSlot.Empty;
                }
            }
        }
    }
}
