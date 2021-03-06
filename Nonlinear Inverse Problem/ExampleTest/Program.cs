﻿using SlaeSolver;
using FEM;
using System.Collections.Generic;
using System;
using System.Numerics;
using NonlinearInverseProblem;
using MathUtilities;
using System.IO;

namespace ExampleTest
{
	class Program
	{
		static double sigma = 1.0;
		static double eps = 1.0e-11;

		static void FEMTest()
		{
			Dictionary<int, Material> materials = new Dictionary<int, Material>()
			{
				{ 0, new Material(sigma) },
				{ 1, new Material(sigma) }
			};

			Dictionary<AreaSide, (ConditionType, Func<double, double, double>)> conditions = new Dictionary<AreaSide, (ConditionType, Func<double, double, double>)>()
			{
				{ AreaSide.Top,		(ConditionType.First, (double r, double z) => r * r * z) },
				{ AreaSide.Left,		(ConditionType.First, (double r, double z) => r * r * z) },
				{ AreaSide.Bottom,	(ConditionType.First, (double r, double z) => r * r * z) },
				{ AreaSide.Right,		(ConditionType.First, (double r, double z) => r * r * z) }
			};

			ImprovedAreaInfo areainfo = new ImprovedAreaInfo();
			areainfo.R0 = 0.0;
			areainfo.Z0 = 0.0;
			areainfo.Width = 100;
			areainfo.FirstLayerHeight = 100;
			areainfo.SecondLayerHeight = 300;

			areainfo.HorizontalStartStep = 10;
			areainfo.HorizontalCoefficient = 1.2;
			areainfo.VerticalStartStep = 10;
			areainfo.VerticalCoefficient = 1.2;
			areainfo.Materials = materials;
			areainfo.Conditions = conditions;

			ImprovedGridBuilder gb = new ImprovedGridBuilder(areainfo);
			gb.Build();

			Func<double, double, double> F = (double r, double z) => -sigma * 4.0 * z;

			FEMProblemInfo info = new FEMProblemInfo();
			info.Points = gb.Points.ToArray();
			info.Materials = materials;
			info.Mesh = gb.Grid;
			info.FB = gb.FB;
			info.F = F;
			info.SolverType = SolverTypes.LOSLU;
			FEMrz fem = new FEMrz(info);
			fem.Solve();

			Console.WriteLine(fem.Info.Points[35].R);
			Console.WriteLine(fem.Info.Points[35].Z);
			Console.WriteLine(fem.Weights[35]);
		}

		static void FEMDeltaTest()
		{
			Dictionary<int, Material> materials = new Dictionary<int, Material>()
			{
				{ 0, new Material(sigma) },
				{ 1, new Material(sigma) }
			};

			Dictionary<AreaSide, (ConditionType, Func<double, double, double>)> Conditions = new Dictionary<AreaSide, (ConditionType, Func<double, double, double>)>()
			{
				{ AreaSide.Top,      (ConditionType.SecondNull, (double r, double z) => 0) },
				{ AreaSide.Left,     (ConditionType.SecondNull, (double r, double z) => 0) },
				{ AreaSide.Bottom,   (ConditionType.First, (double r, double z) => 0) },
				{ AreaSide.Right,    (ConditionType.First, (double r, double z) => 0) }
			};

			AreaInfo areainfo = new AreaInfo();
			areainfo.R0 = 0.0;
			areainfo.Z0 = 0.0;
			areainfo.Width = 10000;
			areainfo.FirstLayerHeight = 100;
			areainfo.SecondLayerHeight = 9900;

			areainfo.HorizontalNodeCount = 500;
			areainfo.FirstLayerVerticalNodeCount = 5;
			areainfo.SecondLayerHeight = 500;
			areainfo.Materials = materials;
			areainfo.Conditions = Conditions;

			GridBuilder gb = new GridBuilder(areainfo);
			gb.Build();

			FEMProblemInfoDelta infoDelta = new FEMProblemInfoDelta();
			infoDelta.Points = gb.Points.ToArray();
			infoDelta.Materials = materials;
			infoDelta.Mesh = gb.Grid;
			infoDelta.FB = gb.FB;
			infoDelta.R0 = 0.0;
			infoDelta.Z0 = 0.0;
			infoDelta.DeltaCoefficient = 1.0 / (2 * Math.PI);
			infoDelta.SolverType = SolverTypes.LOSLU;

			FEMrzDelta femDelta = new FEMrzDelta(infoDelta);
			femDelta.Solver.Eps = 1.0e-11;
			femDelta.Solve();

			Console.WriteLine();
			Console.WriteLine(femDelta.U(new Point(1000, 0)));
			Console.WriteLine();

			Console.WriteLine(femDelta.Solver.Difference);
			Console.WriteLine(femDelta.Solver.IterCount);
		}

		static void ImprovedFEMDeltaTest()
		{
			Dictionary<int, Material> materials = new Dictionary<int, Material>()
			{
				{ 0, new Material(sigma) },
				{ 1, new Material(sigma) }
			};

			Dictionary<AreaSide, (ConditionType, Func<double, double, double>)> Conditions = new Dictionary<AreaSide, (ConditionType, Func<double, double, double>)>()
			{
				{ AreaSide.Top,		(ConditionType.SecondNull, (double r, double z) => 0) },
				{ AreaSide.Left,		(ConditionType.SecondNull, (double r, double z) => 0) },
				{ AreaSide.Bottom,	(ConditionType.First, (double r, double z) => 0) },
				{ AreaSide.Right,		(ConditionType.First, (double r, double z) => 0) }
			};

			ImprovedAreaInfo areainfo = new ImprovedAreaInfo();
			areainfo.R0 = 0.0;
			areainfo.Z0 = 0.0;
			areainfo.Width = 10000;
			areainfo.FirstLayerHeight = 100;
			areainfo.SecondLayerHeight = 9900;

			areainfo.HorizontalStartStep = 0.1;
			areainfo.HorizontalCoefficient = 1.1;
			areainfo.VerticalStartStep = 0.1;
			areainfo.VerticalCoefficient = 1.1;
			areainfo.Materials = materials;
			areainfo.Conditions = Conditions;

			ImprovedGridBuilder gb = new ImprovedGridBuilder(areainfo);
			gb.Build();

			FEMProblemInfoDelta infoDelta = new FEMProblemInfoDelta();
			infoDelta.Points = gb.Points.ToArray();
			infoDelta.Materials = materials;
			infoDelta.Mesh = gb.Grid;
			infoDelta.FB = gb.FB;
			infoDelta.R0 = 0.0;
			infoDelta.Z0 = 0.0;
			infoDelta.DeltaCoefficient = 1.0;
			infoDelta.SolverType = SolverTypes.LOSLU;

			FEMrzDelta femDelta = new FEMrzDelta(infoDelta);
			femDelta.Solver.Eps = 1.0e-8;
			femDelta.Solve();

			Console.WriteLine();
			Console.WriteLine(femDelta.U(new Point(300, 0)));
			Console.WriteLine();

			Console.WriteLine(femDelta.Solver.Difference);
			Console.WriteLine(femDelta.Solver.IterCount);
		}

		static void ImprovedFEMTest()
		{
			Dictionary<int, Material> materials = new Dictionary<int, Material>()
			{
				{ 0, new Material(1.0) },
				{ 1, new Material(1.0) }
			};

			Dictionary<AreaSide, (ConditionType, Func<double, double, double>)> Conditions = new Dictionary<AreaSide, (ConditionType, Func<double, double, double>)>()
			{
				{ AreaSide.TopFirst, (ConditionType.Second, (double r, double z) => 0) },
				{ AreaSide.Bottom,   (ConditionType.First, (double r, double z) => 0) },
				{ AreaSide.Right,    (ConditionType.First, (double r, double z) => 0) }
			};

			AreaInfoWithoutDelta areainfo = new AreaInfoWithoutDelta();
			areainfo.R0 = 0.0;
			areainfo.Z0 = 0.0;
			areainfo.Width = 10000;
			areainfo.FirstLayerHeight = 300;
			areainfo.SecondLayerHeight = 9700;

			areainfo.HorizontalStartStep = 0.1;
			areainfo.HorizontalCoefficient = 1.1;
			areainfo.VerticalStartStep = 0.1;
			areainfo.VerticalCoefficient = 1.1;
			areainfo.Materials = materials;
			areainfo.Conditions = Conditions;

			areainfo.SplitPoint = 0.5;

			GridBuilderWithoutDelta gb = new GridBuilderWithoutDelta(areainfo);
			gb.Build();

			foreach (Edge edge in gb.SB.Edges)
				edge.Function = (double r, double z) => 1.0 / (Math.PI * gb.ClosestSplitPoint * gb.ClosestSplitPoint);

			FEMProblemInfo info = new FEMProblemInfo();
			info.Points = gb.Points.ToArray();
			info.Materials = materials;
			info.Mesh = gb.Grid;
			info.FB = gb.FB;
			info.SB = gb.SB;
			info.F = (double r, double z) => 0.0;
			info.SolverType = SolverTypes.LOSLU;

			FEMrz fem = new FEMrz(info);
			fem.Solver.Eps = 5.0e-15;
			fem.Solve();

			Console.WriteLine();
			Console.WriteLine(fem.U(new Point(300, 0)));
			Console.WriteLine(fem.U(new Point(400, 0)));
			Console.WriteLine(fem.U(new Point(500, 0)));
			Console.WriteLine(fem.U(new Point(1100, 0)));
			Console.WriteLine();

			Console.WriteLine(fem.Solver.Difference);
			Console.WriteLine(fem.Solver.IterCount);
		}

		static void InverseProblemTest()
		{
			using (StreamWriter stream = new StreamWriter("result.csv"))
			{
				Vector3 A = new Vector3(0.0f, 0.0f, 0.0f);
				Vector3 B = new Vector3(100.0f, 0.0f, 0.0f);
				double I = 1.0;

				Source source = new Source(A, B, I);

				Vector3 M1 = new Vector3(200.0f, 0.0f, 0.0f);
				Vector3 N1 = new Vector3(300.0f, 0.0f, 0.0f);

				Vector3 M2 = new Vector3(500.0f, 0.0f, 0.0f);
				Vector3 N2 = new Vector3(600.0f, 0.0f, 0.0f);

				Vector3 M3 = new Vector3(1000.0f, 0.0f, 0.0f);
				Vector3 N3 = new Vector3(1100.0f, 0.0f, 0.0f);

				List<Receiver> receivers = new List<Receiver>() { new Receiver(M1, N1), new Receiver(M2, N2), new Receiver(M3, N3) };

				Problem.ProblemInfo info = new Problem.ProblemInfo();
				info.Source = source;
				info.Receivers = receivers;
				info.sigma1 = 0.01;
				info.sigma2 = 0.1;

				// True parameter
				double trueP = 100;
				info.h = trueP;
				Console.WriteLine("Start to solve problem for true parameter");
				info.TrueV = Problem.DirectProblem(info, eps);

				Console.Write($"Values for parameter {trueP}: ");
				stream.Write($"True parameter; {trueP}\n");
				stream.Write($"True V;");

				for (int k = 0; k < receivers.Count; k++)
				{
					Console.Write(info.TrueV[k]);
					stream.Write($"{info.TrueV[k]};");
				}

				Console.WriteLine();
				Console.WriteLine();
				stream.Write("\n");

				// Initial parameter
				double initialP = 200;
				info.h = initialP;
				Console.WriteLine("Start to solve problem for initial parameter");
				info.V = Problem.DirectProblem(info, eps);

				Console.Write($"Values for parameter {initialP}: ");
				stream.Write($"Initial parameter; {initialP}\n");
				stream.Write($"Initial V;");

				for (int k = 0; k < receivers.Count; k++)
				{
					Console.Write(info.V[k]);
					stream.Write($"{info.V[k]};");
				}

				Console.WriteLine();
				Console.WriteLine();
				stream.Write("\n");


				// Initial Functional value
				double F = Problem.Functional(info.V, info.TrueV);

				// First table row
				Console.WriteLine("Start to solve nonlinear inverse problem");
				Console.WriteLine($"Fuctional value - {F}");
				Console.WriteLine();

				stream.Write("\nParameter; V1; V2; V3; F\n");
				stream.Write($"{info.h};");
				for (int k = 0; k < receivers.Count; k++)
					stream.Write($"{info.V[k]};");
				stream.Write($"{F}\n");

				int i = 0;
				while (F > 1.0e-7 && i < 30)
				{
					double dh = Problem.InverseProblem(info, eps, 10);
					info.h += dh;

					Console.WriteLine($"New h - {info.h}, New functional value - {F}");
					Console.WriteLine();
					info.V = Problem.DirectProblem(info, eps);
					F = Problem.Functional(info.V, info.TrueV);
					i++;

					stream.Write($"{info.h};");
					for (int k = 0; k < receivers.Count; k++)
						stream.Write($"{info.V[k]};");
					stream.Write($"{F}\n");
				}

				Console.WriteLine($"Result h - {info.h}, Result functional value - {F}");

				stream.Write($"{info.h};");
				for (int k = 0; k < receivers.Count; k++)
					stream.Write($"{info.V[k]};");
				stream.Write($"{F}\n");
			}
		}

		static void Main(string[] args)
		{
			InverseProblemTest();
		}
	}
}
