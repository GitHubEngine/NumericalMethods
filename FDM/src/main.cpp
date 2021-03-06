#include "MeshBuilder/MeshBuilder.h"
#include "MatrixBuilder/MatrixBuilder.h"

#include <SLAE/SLAE.h>

#include <iostream>
#include <vector>

using namespace std;

void PrintTest(string filename, vector<double>& x, int kx)
{
	ofstream out(filename);

	int size = x.size();
	int rowCount = size / kx;
	for (int i = 0; i < rowCount; i++)
	{
		for (int j = 0; j < kx; j++)
			out << x[i * kx + j] << ';';

		out << endl;
	}
}

int main()
{
	vector<Interval> intervalsX, intervalsY;
	ReadIntervals("input/intervalsX.txt", intervalsX);
	ReadIntervals("input/intervalsY.txt", intervalsY);

	vector<vector<int>> areas;
	ReadAreaMatrix("input/areaMatrix.txt", areas);

	vector<BoundaryCondition> conds;
	ReadBoundaryConds("input/boundary.txt", conds);

	int kx = CountNodes(intervalsX);
	int ky = CountNodes(intervalsY);

	IntervalNumbering(intervalsX);
	IntervalNumbering(intervalsY);

	BoundaryCondsNumbering(intervalsX, intervalsY, conds);

	vector<double> x(kx), y(ky);
	BuildMesh(intervalsX, x);
	BuildMesh(intervalsY, y);

	SLAE::Matrix A;
	vector<double> b(kx * ky);

	SLAE::InitMatrix(A, kx * ky, kx);
	BuildMatrix(A, b, areas, intervalsX, intervalsY, conds, x, y, kx, ky);
	BoundaryConditions(A, b, x, y, conds);

	vector<double> x0(kx * ky), r(kx * ky);
	int k = SLAE::Zeidel(A, b, x0, r, 1.5);

	PrintTest("result.csv", x0, kx);
	return 0;
}