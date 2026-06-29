from __future__ import annotations

import heapq
from dataclasses import dataclass, field
from typing import Dict, List, Optional, Set, Tuple

State = Tuple[Tuple[int, ...], ...]

INITIAL_STATE: State = ((2, 8, 3), (1, 6, 4), (7, 0, 5))
GOAL_STATE: State = ((1, 2, 3), (8, 0, 4), (7, 6, 5))

GOAL_POS: Dict[int, Tuple[int, int]] = {}
for r in range(3):
    for c in range(3):
        tile = GOAL_STATE[r][c]
        if tile != 0:
            GOAL_POS[tile] = (r, c)


@dataclass(order=True)
class Node:
    f: int
    g: int
    state: State = field(compare=False)
    parent: Optional["Node"] = field(default=None, compare=False)
    move: str = field(default="", compare=False)


def state_to_str(state: State) -> str:
    lines = []
    for row in state:
        lines.append(" ".join("_" if x == 0 else str(x) for x in row))
    return "\n".join(lines)


def manhattan_distance(state: State) -> int:
    total = 0
    for r in range(3):
        for c in range(3):
            tile = state[r][c]
            if tile != 0:
                gr, gc = GOAL_POS[tile]
                total += abs(r - gr) + abs(c - gc)
    return total


def find_blank(state: State) -> Tuple[int, int]:
    for r in range(3):
        for c in range(3):
            if state[r][c] == 0:
                return r, c
    raise ValueError("未找到空格")


def get_neighbors(state: State) -> List[Tuple[State, str]]:
    r, c = find_blank(state)
    moves = [(-1, 0, "上"), (1, 0, "下"), (0, -1, "左"), (0, 1, "右")]
    neighbors: List[Tuple[State, str]] = []
    state_list = [list(row) for row in state]

    for dr, dc, direction in moves:
        nr, nc = r + dr, c + dc
        if 0 <= nr < 3 and 0 <= nc < 3:
            new_state = [row[:] for row in state_list]
            new_state[r][c], new_state[nr][nc] = new_state[nr][nc], new_state[r][c]
            neighbors.append((tuple(tuple(row) for row in new_state), direction))
    return neighbors


def astar(initial: State, goal: State) -> Optional[Node]:
    if initial == goal:
        return Node(f=0, g=0, state=initial)

    h0 = manhattan_distance(initial)
    start = Node(f=h0, g=0, state=initial)
    open_heap: List[Node] = [start]
    open_set: Dict[State, int] = {initial: h0}
    closed: Set[State] = set()

    while open_heap:
        current = heapq.heappop(open_heap)

        if current.state in closed:
            continue
        closed.add(current.state)

        if current.state == goal:
            return current

        for next_state, move in get_neighbors(current.state):
            if next_state in closed:
                continue

            g = current.g + 1
            h = manhattan_distance(next_state)
            f = g + h

            if next_state in open_set and f >= open_set[next_state]:
                continue

            open_set[next_state] = f
            child = Node(f=f, g=g, state=next_state, parent=current, move=move)
            heapq.heappush(open_heap, child)

    return None


def reconstruct_path(goal_node: Node) -> List[Node]:
    path: List[Node] = []
    node: Optional[Node] = goal_node
    while node:
        path.append(node)
        node = node.parent
    path.reverse()
    return path


def print_solution_path(path: List[Node]) -> None:
    for i, node in enumerate(path):
        print(f"S{i}:")
        print(state_to_str(node.state))

        if i < len(path) - 1:
            print("    |")
            print(f"    | ({path[i + 1].move})")
            print("    v")
            print()


def main() -> None:
    result = astar(INITIAL_STATE, GOAL_STATE)
    if result is None:
        print("无解")
        return
    print_solution_path(reconstruct_path(result))


if __name__ == "__main__":
    main()
