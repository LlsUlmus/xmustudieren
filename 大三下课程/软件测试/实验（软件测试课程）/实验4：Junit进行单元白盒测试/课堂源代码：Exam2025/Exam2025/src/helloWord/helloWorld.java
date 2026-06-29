package helloWord;

public class helloWorld {
	
	public int SayHello(int X) {
		if (X>=0) return X+1;
		else return X*X;
	}

	public static void main(String[] args) {
		 int X1,X2,X3;
		helloWorld hl;
		
		hl=new helloWorld();
		X1=-2; X2=0; X3=2;
		
		System.out.println("X1=" + X1 + ", R1="+ hl.SayHello(X1));
		System.out.println("X2=" + X2 + ", R2="+ hl.SayHello(X2));
		System.out.println("X3=" + X3 + ", R3="+ hl.SayHello(X3));
		
		
	}

}
