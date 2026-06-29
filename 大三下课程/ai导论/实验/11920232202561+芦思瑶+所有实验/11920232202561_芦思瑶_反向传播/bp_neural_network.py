import os
import sys
import numpy as np

LEARNING_RATE = 0.03
HIDDEN_SIZE = 10
N_EPOCHS = 10000
N_RUNS = 10


def get_base_dir():
    if getattr(sys, "frozen", False):
        return os.path.dirname(sys.executable)
    return os.path.dirname(os.path.abspath(__file__))


def sigmoid(x):
    return 1.0 / (1.0 + np.exp(-x))


def sigmoid_derivative(a):
    return a * (1.0 - a)


def initialize_parameters(input_size, hidden_size, output_size):
    return {
        "W1": np.random.randn(input_size, hidden_size) * 0.01,
        "b1": np.zeros((1, hidden_size)),
        "W2": np.random.randn(hidden_size, output_size) * 0.01,
        "b2": np.zeros((1, output_size)),
    }


def forward_propagation(X, parameters):
    W1, b1 = parameters["W1"], parameters["b1"]
    W2, b2 = parameters["W2"], parameters["b2"]
    z1 = np.dot(X, W1) + b1
    a1 = sigmoid(z1)
    z2 = np.dot(a1, W2) + b2
    a2 = sigmoid(z2)
    cache = {"A1": a1, "A2": a2}
    return a2, cache


def compute_loss(Y, A2):
    m = Y.shape[0]
    eps = 1e-8
    loss = -np.sum(Y * np.log(A2 + eps) + (1 - Y) * np.log(1 - A2 + eps)) / m
    return loss


def backward_propagation(parameters, cache, X, Y):
    m = X.shape[0]
    W2 = parameters["W2"]
    a1, a2 = cache["A1"], cache["A2"]

    dZ2 = a2 - Y
    dW2 = np.dot(a1.T, dZ2) / m
    db2 = np.sum(dZ2, axis=0, keepdims=True) / m

    dZ1 = np.dot(dZ2, W2.T) * sigmoid_derivative(a1)
    dW1 = np.dot(X.T, dZ1) / m
    db1 = np.sum(dZ1, axis=0, keepdims=True) / m

    return {"dW1": dW1, "db1": db1, "dW2": dW2, "db2": db2}


def update_parameters(parameters, grads, learning_rate):
    parameters["W1"] -= learning_rate * grads["dW1"]
    parameters["b1"] -= learning_rate * grads["db1"]
    parameters["W2"] -= learning_rate * grads["dW2"]
    parameters["b2"] -= learning_rate * grads["db2"]
    return parameters


def convert_to_one_hot(labels, num_classes=3):
    indices = labels.astype(int) - 1
    return np.eye(num_classes)[indices]


def load_dataset(train_path, test_path):
    train_data = np.loadtxt(train_path, delimiter=",")
    test_data = np.loadtxt(test_path, delimiter=",")
    X_train = train_data[:, :4]
    Y_train = convert_to_one_hot(train_data[:, 4])
    X_test = test_data[:, :4]
    y_test = test_data[:, 4].astype(int)
    return X_train, Y_train, X_test, y_test


def train_model(X_train, Y_train, X_test, y_test, hidden_size, learning_rate, n_epochs):
    input_size = X_train.shape[1]
    output_size = Y_train.shape[1]
    parameters = initialize_parameters(input_size, hidden_size, output_size)

    for epoch in range(n_epochs):
        # 前向传播
        A2, cache = forward_propagation(X_train, parameters)
        cost = compute_loss(Y_train, A2)

        if epoch % 1000 == 0:
            print(f"  Epoch {epoch}/{n_epochs} - Loss: {cost:.6f} - Learning Rate: {learning_rate}")

        # 反向传播
        grads = backward_propagation(parameters, cache, X_train, Y_train)
        # 更新参数
        parameters = update_parameters(parameters, grads, learning_rate)

    A2, _ = forward_propagation(X_test, parameters)
    predictions = np.argmax(A2, axis=1) + 1
    accuracy = np.mean(predictions == y_test)
    return accuracy, cost


def main():
    base_dir = get_base_dir()
    train_path = os.path.join(base_dir, "Iris-train.txt")
    test_path = os.path.join(base_dir, "Iris-test.txt")

    X_train, Y_train, X_test, y_test = load_dataset(train_path, test_path)
    accuracies = []
    losses = []

    for i in range(N_RUNS):
        print(f"第 {i + 1} 次训练:")
        accuracy, loss = train_model(
            X_train, Y_train, X_test, y_test,
            hidden_size=HIDDEN_SIZE,
            learning_rate=LEARNING_RATE,
            n_epochs=N_EPOCHS,
        )
        print(f"  训练 {i + 1}: 准确率 = {accuracy}, 损失 = {loss}\n")
        accuracies.append(accuracy)
        losses.append(loss)

    print("所有训练的最终结果:")
    print(f"平均准确率: {np.mean(accuracies)}")
    print(f"准确率的标准差: {np.std(accuracies)}")
    print(f"平均损失: {np.mean(losses)}")
    print(f"损失的标准差: {np.std(losses)}")


if __name__ == "__main__":
    main()
