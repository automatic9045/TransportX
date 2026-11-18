using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using TransportX.Diagnostics;
using TransportX.Physics;

namespace TransportX.Scripting.Data.Rendering
{
    [XmlRoot]
    public class CrossSection
    {
        [XmlArrayItem("Vertex")]
        public List<CrossSectionVertex> Vertices = [];

        [XmlArrayItem("Surface")]
        public List<CrossSectionSurface> Surfaces = [];

        [XmlArrayItem]
        public List<Material> Materials = [];

        [XmlArrayItem]
        public List<ColliderMaterial> ColliderMaterials = [];

        [XmlIgnore]
        public Error[] Errors =>
        [
            .. Vertices.SelectMany(vertex => vertex.Errors),
            .. Surfaces.SelectMany(surface => surface.AllErrors),
            .. Materials.SelectMany(material => material.AllErrors),
            .. ColliderMaterials.SelectMany(colliderMaterial => colliderMaterial.Errors),
        ];
    }
}
