using System.Collections.Generic;

namespace StereoKit.Framework
{
	public class StaticScene
	{
		internal List<StaticSceneItem> _items = new List<StaticSceneItem>();

		public void AddModel(Model model, Matrix at)
		{
			if (model == null) return;

			foreach (ModelNode node in model.Nodes)
			{
				if (node.Mesh == null) continue;
				
				string name = node.Name;

				bool visible = 
					name.Contains("[invisible]") == false ||
					(node.Info.Contains("visible") && node.Info["visible"] == "true");
				bool solid = 
					name.Contains("[intangible]") == false ||
					(node.Info.Contains("solid") && node.Info["solid"] == "true");

				AddMesh(node.Mesh, node.Material, node.ModelTransform * at, visible, solid);
			}
		}

		public void AddMesh(Mesh mesh, Material material, Matrix at, bool solid = true, bool visible = true)
		{
			StaticSceneItem item = new StaticSceneItem();
			item.visible      = visible;
			item.solid        = solid;
			item.material     = material;
			item.mesh         = mesh;
			item.transform    = at;
			item.invTransform = at.Inverse;
			_items.Add(item);
		}

		public bool Raycast(Ray worldRay, out Ray at)
		{
			float closest = float.MaxValue;
			at = default;
			for (int i = 0; i < _items.Count; i++)
			{
				if (_items[i].Raycast(worldRay, out Ray curr))
				{
					float dist = Vec3.DistanceSq(curr.position, worldRay.position);
					if (dist < closest)
					{
						closest = dist;
						at      = curr;
					}
				}
			}
			return closest != float.MaxValue;
		}

		public void Draw()
		{
			for (int i = 0; i < _items.Count; i++)
				_items[i].Draw();
		}
	}

	struct StaticSceneItem
	{
		internal Matrix   transform;
		internal Matrix   invTransform;
		internal Mesh     mesh;
		internal Material material;
		internal bool     solid;
		internal bool     visible;

		public bool Raycast(Ray worldRay, out Ray intersection)
		{
			if (solid)
			{
				bool result = mesh.Intersect(invTransform.Transform(worldRay), out intersection);
				if (result) intersection = transform.Transform(intersection);
				return result;
			}
			else
			{
				intersection = default;
				return false;
			}
		}

		public void Draw() { if (visible) mesh.Draw(material, transform); }
	}
}
