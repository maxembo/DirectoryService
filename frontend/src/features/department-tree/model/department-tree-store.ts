import { type DepartmentId } from "@/entities/departments";
import { create } from "zustand";
import { createJSONStorage, persist } from "zustand/middleware";

export type DepartmentTreeId = string;

type DepartmentTreeState = {
	selectedId: DepartmentId | null;
	expandedIds: DepartmentId[];
};

type DepartmentTreeStates = Record<
	DepartmentTreeId,
	DepartmentTreeState | undefined
>;

const initialState: DepartmentTreeState = {
	selectedId: null,
	expandedIds: [],
};

const DEFAULT_STATE_ID = "__default__";

const initialStates: DepartmentTreeStates = {};

const resolveStateId = (stateId?: DepartmentTreeId) =>
	stateId ?? DEFAULT_STATE_ID;

const getDepartmentTreeState = (
	states: DepartmentTreeStates,
	stateId?: DepartmentTreeId,
) => states[resolveStateId(stateId)] ?? initialState;

const useDepartmentTreeStore = create<DepartmentTreeStates>()(
	persist(() => ({ ...initialStates }), {
		name: "department-tree-store",
		storage: createJSONStorage(() => localStorage),
		partialize: (states) => {
			const tree = states[DEFAULT_STATE_ID];

			if (!tree) return {};

			return {
				[DEFAULT_STATE_ID]: {
					...tree,
					selectedId: null,
				},
			};
		},
	}),
);

export const useDepartmentTreeSelectedId = (stateId?: DepartmentTreeId) =>
	useDepartmentTreeStore(
		(state) => getDepartmentTreeState(state, stateId).selectedId,
	);

export const setDepartmentTreeSelectedId = (
	selectedId: DepartmentId,
	stateId?: DepartmentTreeId,
) =>
	useDepartmentTreeStore.setState((states) => ({
		[resolveStateId(stateId)]: {
			...getDepartmentTreeState(states, stateId),
			selectedId,
		},
	}));

export const useDepartmentTreeExpandedIds = (stateId?: DepartmentTreeId) =>
	useDepartmentTreeStore(
		(state) => getDepartmentTreeState(state, stateId).expandedIds,
	);

export const toggleDepartmentTreeExpandedId = (
	departmentId: DepartmentId,
	hasChildren: boolean,
	stateId?: DepartmentTreeId,
) => {
	if (!hasChildren) return;

	const id = resolveStateId(stateId);
	useDepartmentTreeStore.setState((states) => {
		const currentState = getDepartmentTreeState(states, stateId);
		const isExpanded = currentState.expandedIds.includes(departmentId);

		return {
			...states,
			[id]: {
				...currentState,
				expandedIds: isExpanded
					? currentState.expandedIds.filter((id) => id !== departmentId)
					: [...currentState.expandedIds, departmentId],
			},
		};
	});
};

export const collapseAllDepartments = (stateId?: DepartmentTreeId) =>
	useDepartmentTreeStore.setState((states) => ({
		[resolveStateId(stateId)]: {
			...getDepartmentTreeState(states, stateId),
			expandedIds: [],
		},
	}));

export const resetDepartmentTreeData = () =>
	useDepartmentTreeStore.setState((states) => {
		const nextStates: DepartmentTreeStates = {};

		for (const [id, state] of Object.entries(states)) {
			nextStates[id] = state
				? {
						...state,
						expandedIds: [],
					}
				: undefined;
		}

		return nextStates;
	});
