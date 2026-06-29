package PathTestExam;

public class BookExam {
	public int ComputeX(int A, int B, int X) {
		if (A > 1 && B == 0)
			X = X / A;
		if (A == 2 || X > 1)
			X = X + 1;
		return X;
	}
}
