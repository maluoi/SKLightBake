using StereoKit;
using StereoKit.Framework;
using System;
using System.Threading.Tasks;

namespace SKLightBake;

class Program
{
	static StaticScene staticScene;
	static BakedScene  bakedScene;
	static float       bakedSamples = 16;

	static Mesh     floorMesh;
	static Material floorMaterial;
	static Mesh     cubeMesh;
	static Material cubeMaterial;

	static void Main(string[] args)
	{
		// Initialize StereoKit
		if (!SK.Initialize())
			Environment.Exit(1);

		floorMesh   = Mesh.GeneratePlane(V.XY(10, 10), 20);
		cubeMesh    = Mesh.GenerateCube (Vec3.One, 8);
		floorMaterial = Material.Default.Copy();
		floorMaterial[MatParamName.DiffuseTex] = Tex.FromFile("test.png");
		cubeMaterial = Material.Default.Copy();
		cubeMaterial[MatParamName.DiffuseTex] = Tex.FromFile("floor.png");

		staticScene = GenerateScene();

		bakedScene  = new BakedScene();
		bakedScene.AddDirectionalLight(V.XYZ(-1, -1, -1), Color.White, 1);
		bakedScene.SetSky(Renderer.SkyLight);
		bakedScene.Bake(staticScene, 0);
		Task.Run(() => bakedScene.Bake(staticScene, (int)bakedSamples));

		// Core application loop
		while (SK.Step(() =>
		{
			bakedScene.Draw();
			WindowBakeSettings();
		}));
		SK.Shutdown();
	}

	static StaticScene GenerateScene()
	{
		float height = -1.6f;
		if (World.HasBounds)
			height = World.BoundsPose.position.y;

		Random      r      = new Random();
		StaticScene result = new StaticScene();
		result.AddMesh(floorMesh, floorMaterial, Matrix.T(0,height,0));
		for (int i = 0; i < 20; i++)
		{
			Vec3 at    = new Vec3(r.NextSingle() * 10 - 5, r.NextSingle() * 4 + height, r.NextSingle() * 10 - 5);
			Vec3 scale = new Vec3(r.NextSingle() * 3 + 0.5f, r.NextSingle() * 3 + 0.5f, r.NextSingle() * 3 + 0.5f);
			result.AddMesh(cubeMesh, cubeMaterial, Matrix.TS(
				at + V.XYZ(0,scale.y/2.0f, 0), scale ));
		}

		return result;
	}

	static Pose bakeSettingsPose = new Pose(0, 0, -0.5f, Quat.LookDir(0, 0, 1));
	static void WindowBakeSettings()
	{
		UI.WindowBegin("Bake Settings", ref bakeSettingsPose);
		UI.Label("Samples"); UI.SameLine(); UI.HSlider("Samples", ref bakedSamples, 0, 512, 1);

		UI.PushEnabled(!bakedScene.Baking);
		if (UI.Button("Regenerate"))
		{
			staticScene = GenerateScene();
			bakedScene.Bake(staticScene, 0);
		}

		UI.SameLine();
		if (UI.Button("Bake"))
			Task.Run(() => bakedScene.Bake(staticScene, (int)bakedSamples));

		UI.PopEnabled();
		if (bakedScene.Baking)
		{
			UI.SameLine();
			UI.Label("Baking...");
			UI.ProgressBar(bakedScene.BakingProgress);
		}
		UI.WindowEnd();
	}
}
