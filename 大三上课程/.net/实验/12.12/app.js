const { createApp } = Vue;

createApp({
    data() {
        return {
            patternType: 'pyramid',
            rows: 6,
            pyramidPattern: [],
            trianglePattern: [],
            invertedPattern: [],
            diamondPattern: [],
            pascalPattern: [],
            numberSquarePattern: [],
            spiralPattern: []
        };
    },
    mounted() {
        this.generatePattern();
    },
    methods: {
        generatePattern() {
            switch(this.patternType) {
                case 'pyramid':
                    this.generatePyramid();
                    break;
                case 'triangle':
                    this.generateTriangle();
                    break;
                case 'inverted':
                    this.generateInverted();
                    break;
                case 'diamond':
                    this.generateDiamond();
                    break;
                case 'pascal':
                    this.generatePascal();
                    break;
                case 'numberSquare':
                    this.generateNumberSquare();
                    break;
                case 'spiral':
                    this.generateSpiral();
                    break;
            }
        },
        
        // 金字塔：每行递增数字，居中显示
        generatePyramid() {
            this.pyramidPattern = [];
            for (let i = 1; i <= this.rows; i++) {
                const row = [];
                for (let j = 1; j <= i; j++) {
                    row.push(j);
                }
                this.pyramidPattern.push(row);
            }
        },
        
        // 正三角形：每行显示1到i的数字
        generateTriangle() {
            this.trianglePattern = [];
            for (let i = 1; i <= this.rows; i++) {
                const row = [];
                for (let j = 1; j <= i; j++) {
                    row.push(j);
                }
                this.trianglePattern.push(row);
            }
        },
        
        // 倒三角形：从rows递减到1
        generateInverted() {
            this.invertedPattern = [];
            for (let i = this.rows; i >= 1; i--) {
                const row = [];
                for (let j = 1; j <= i; j++) {
                    row.push(j);
                }
                this.invertedPattern.push(row);
            }
        },
        
        // 对称钻石：上下对称的数字排列
        generateDiamond() {
            this.diamondPattern = [];
            const totalRows = this.rows * 2 - 1;
            
            // 上半部分（包括中间）
            for (let i = 1; i <= this.rows; i++) {
                const row = [];
                for (let j = 1; j <= i; j++) {
                    row.push(j);
                }
                this.diamondPattern.push(row);
            }
            
            // 下半部分（倒序）
            for (let i = this.rows - 1; i >= 1; i--) {
                const row = [];
                for (let j = 1; j <= i; j++) {
                    row.push(j);
                }
                this.diamondPattern.push(row);
            }
        },
        
        // 杨辉三角（Pascal's Triangle）
        generatePascal() {
            this.pascalPattern = [];
            for (let i = 0; i < this.rows; i++) {
                const row = [];
                for (let j = 0; j <= i; j++) {
                    row.push(this.combination(i, j));
                }
                this.pascalPattern.push(row);
            }
        },
        
        // 计算组合数 C(n, k)
        combination(n, k) {
            if (k === 0 || k === n) return 1;
            if (k > n - k) k = n - k;
            let result = 1;
            for (let i = 0; i < k; i++) {
                result = result * (n - i) / (i + 1);
            }
            return result;
        },
        
        // 数字正方形：螺旋排列的数字
        generateNumberSquare() {
            this.numberSquarePattern = [];
            let num = 1;
            for (let i = 0; i < this.rows; i++) {
                const row = [];
                for (let j = 0; j < this.rows; j++) {
                    row.push(num++);
                }
                this.numberSquarePattern.push(row);
            }
        },
        
        // 数字螺旋：从中心向外螺旋排列
        generateSpiral() {
            const size = this.rows;
            this.spiralPattern = Array(size).fill(null).map(() => Array(size).fill(0));
            
            let num = 1;
            let top = 0, bottom = size - 1, left = 0, right = size - 1;
            
            while (top <= bottom && left <= right) {
                // 从左到右
                for (let i = left; i <= right; i++) {
                    this.spiralPattern[top][i] = num++;
                }
                top++;
                
                // 从上到下
                for (let i = top; i <= bottom; i++) {
                    this.spiralPattern[i][right] = num++;
                }
                right--;
                
                // 从右到左
                if (top <= bottom) {
                    for (let i = right; i >= left; i--) {
                        this.spiralPattern[bottom][i] = num++;
                    }
                    bottom--;
                }
                
                // 从下到上
                if (left <= right) {
                    for (let i = bottom; i >= top; i--) {
                        this.spiralPattern[i][left] = num++;
                    }
                    left++;
                }
            }
        }
    }
}).mount('#app');

