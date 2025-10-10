using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Assimp;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Bus.Common.Rendering
{
    internal class AssimpModelFactory
    {
        // 参考: https://github.com/assimp/assimp/blob/master/samples/SimpleTexturedDirectx11/SimpleTexturedDirectx11/ModelLoader.cpp 

        private readonly ID3D11Device Device;
        private readonly ID3D11DeviceContext Context;

        private readonly AssimpContext Importer = new AssimpContext();
        private readonly List<TextureInfo> LoadedTextures = new List<TextureInfo>();

        public AssimpModelFactory(ID3D11Device device, ID3D11DeviceContext context)
        {
            Device = device;
            Context = context;
        }

        public Model FromFile(string filePath)
        {
            string baseDirectory = Path.GetDirectoryName(filePath)!;
            string extension = Path.GetExtension(filePath);

            PostProcessSteps postProcessSteps = PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.Triangulate | PostProcessSteps.GenerateNormals;
            Scene scene = Importer.ImportFile(filePath, postProcessSteps);

            List<Mesh> meshes = scene.Meshes.ConvertAll(assimpMesh =>
            {
                Vertex[] vertices = assimpMesh.Vertices
                    .Select((assimpVertex, i) =>
                    {
                        Vertex vertex = new Vertex()
                        {
                            X = assimpVertex.X,
                            Y = assimpVertex.Y,
                            Z = assimpVertex.Z,
                        };

                        if (assimpMesh.HasTextureCoords(0))
                        {
                            Vector3 assimpTextureCoord = assimpMesh.TextureCoordinateChannels[0][i];
                            vertex.TextureCoord = new Vector2(assimpTextureCoord.X, assimpTextureCoord.Y);
                        }

                        return vertex;
                    })
                    .ToArray();

                List<int> indices = new List<int>();
                foreach (Face face in assimpMesh.Faces)
                {
                    indices.AddRange(face.Indices);
                }

                List<ID3D11ShaderResourceView> textures = new List<ID3D11ShaderResourceView>();
                if (0 <= assimpMesh.MaterialIndex)
                {
                    Material material = scene.Materials[assimpMesh.MaterialIndex];

                    List<ID3D11ShaderResourceView> diffuseMaps = LoadMaterialTextures(material, TextureType.Diffuse, "texture_diffuse", scene, baseDirectory);
                    textures.AddRange(diffuseMaps);
                }

                return Mesh.Create(Device, vertices, indices.ToArray(), textures);
            });

            return new Model(meshes, LoadedTextures.ConvertAll(x => x.Texture));
        }

        private List<ID3D11ShaderResourceView> LoadMaterialTextures(Material material, TextureType type, string typeName, Scene scene, string baseDirectory)
        {
            List<ID3D11ShaderResourceView> textures = new List<ID3D11ShaderResourceView>();
            for (int i = 0; i < material.GetMaterialTextureCount(type); i++)
            {
                material.GetMaterialTexture(type, i, out TextureSlot slot);

                TextureInfo? loadedTexture = LoadedTextures.Find(texture => texture.FilePath == slot.FilePath);
                if (loadedTexture is not null)
                {
                    textures.Add(loadedTexture.Texture);
                }
                else
                {
                    ID3D11ShaderResourceView texture;
                    EmbeddedTexture? embeddedTexture = scene.GetEmbeddedTexture(slot.FilePath);
                    if (embeddedTexture is not null)
                    {
                        texture = LoadEmbeddedTexture(embeddedTexture);
                    }
                    else
                    {
                        string filePath = Path.Combine(baseDirectory, slot.FilePath);

                        int hr = NativeMethods.CreateWICTextureFromFile_(Device.NativePointer, Context.NativePointer, filePath, out _, out nint textureView);
                        if (hr != 0)
                        {
                            Marshal.ThrowExceptionForHR(hr);
                        }

                        texture = new ID3D11ShaderResourceView(textureView);
                    }

                    TextureInfo textureInfo = new TextureInfo(typeName, slot.FilePath, texture);

                    textures.Add(textureInfo.Texture);
                    LoadedTextures.Add(textureInfo);
                }
            }

            return textures;
        }

        private ID3D11ShaderResourceView LoadEmbeddedTexture(EmbeddedTexture embeddedTexture)
        {
            if (embeddedTexture.Height != 0)
            {
                Texture2DDescription desc = new Texture2DDescription()
                {
                    Width = (uint)embeddedTexture.Width,
                    Height = (uint)embeddedTexture.Height,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    Format = Format.B8G8R8A8_UNorm,
                    BindFlags = BindFlags.ShaderResource,
                    CPUAccessFlags = 0,
                    MiscFlags = 0,
                };

                nint nativeData = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(byte)) * embeddedTexture.CompressedData.Length);
                Marshal.Copy(embeddedTexture.CompressedData, 0, nativeData, embeddedTexture.CompressedData.Length);

                SubresourceData subresourceData = new SubresourceData()
                {
                    DataPointer = nativeData,
                    RowPitch = (uint)(embeddedTexture.Width * 4),
                    SlicePitch = (uint)(embeddedTexture.Width * embeddedTexture.Height * 4),
                };

                ID3D11Texture2D texture2D = Device.CreateTexture2D(desc, subresourceData);
                ID3D11ShaderResourceView texture = Device.CreateShaderResourceView(texture2D);
                
                return texture;
            }

            int hr = NativeMethods.CreateWICTextureFromMemory_(Device.NativePointer, Context.NativePointer, embeddedTexture.CompressedData, embeddedTexture.Width, out _, out nint textureView);
            if (hr != 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            return new ID3D11ShaderResourceView(textureView);
        }


        private class TextureInfo
        {
            public string Type { get; }
            public string FilePath { get; }
            public ID3D11ShaderResourceView Texture { get; }

            public TextureInfo(string type, string filePath, ID3D11ShaderResourceView texture)
            {
                Type = type;
                FilePath = filePath;
                Texture = texture;
            }
        }
    }
}
