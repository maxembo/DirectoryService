import { departmentsApi } from "@/entities/departments/api/api";
import {
	DepartmentId,
	DepartmentTreeDto,
} from "@/entities/departments/model/types";
import { create } from "zustand";
import { createJSONStorage, persist } from "zustand/middleware";

export type DepartmentTreeId = string;

type DepartmentTreeState = {
	selectedId: DepartmentId | null;
	expandedIds: DepartmentId[];
	loadingIds: DepartmentId[];
	childrenByParentId: Record<DepartmentId, DepartmentTreeDto[]>;
	nextPageByParentId: Record<DepartmentId, number | null>;
};

type DepartmentTreeStates = Record<
	DepartmentTreeId,
	DepartmentTreeState | undefined
>;

const initialState: DepartmentTreeState = {
	selectedId: null,
	expandedIds: [],
	childrenByParentId: {},
	loadingIds: [],
	nextPageByParentId: {},
};

const DEFAULT_STATE_ID = "__default__";

const initialStates: DepartmentTreeStates = {};

const resolveStateId = (stateId?: DepartmentTreeId) =>
	stateId ?? DEFAULT_STATE_ID;

const getOrCreate = (
	states: DepartmentTreeStates,
	stateId?: DepartmentTreeId,
) => {
	const id = resolveStateId(stateId);

	if (!states[id]) {
		states[id] = { ...initialState };
	}

	return states[id];
};

const useDepartmentTreeStore = create<DepartmentTreeStates>()(
	persist(() => ({ ...initialStates }), {
		name: "department-tree-store",
		storage: createJSONStorage(() => localStorage),
		partialize: (states) => {
			const tree = states[DEFAULT_STATE_ID];

			if (!tree) return;

			return {
				[DEFAULT_STATE_ID]: {
					...tree,
					selectedId: null,
					loadingIds: [],
				},
			};
		},
	}),
);

export const useDepartmentTreeSelectedId = (stateId?: DepartmentTreeId) =>
	useDepartmentTreeStore((state) => getOrCreate(state, stateId).selectedId);

export const setDepartmentTreeSelectedId = (
	selectedId: DepartmentId,
	stateId?: DepartmentTreeId,
) =>
	useDepartmentTreeStore.setState((states) => ({
		[resolveStateId(stateId)]: {
			...getOrCreate(states, stateId),
			selectedId,
		},
	}));

export const useDepartmentTreeExpandedIds = (stateId?: DepartmentTreeId) =>
	useDepartmentTreeStore((state) => getOrCreate(state, stateId).expandedIds);

export const toggleDepartmentTreeExpandedId = async (
	parentId: DepartmentId,
	hasChildren: boolean,
	stateId?: DepartmentTreeId,
) => {
	if (!hasChildren) return;

	const id = resolveStateId(stateId);
	const currentState = getOrCreate(useDepartmentTreeStore.getState(), stateId);
	const isExpanded = currentState.expandedIds.includes(parentId);

	useDepartmentTreeStore.setState((states) => {
		const currentState = getOrCreate(states, stateId);
		const isExpanded = currentState.expandedIds.includes(parentId);

		return {
			...states,
			[id]: {
				...currentState,
				expandedIds: isExpanded
					? currentState.expandedIds.filter((id) => id !== parentId)
					: [...currentState.expandedIds, parentId],
			},
		};
	});
	if (!isExpanded) {
		await loadNextDepartmentChildrenPage(parentId, stateId);
	}
};

export const useDepartmentTreeChildrenByParentId = (
	stateId?: DepartmentTreeId,
) =>
	useDepartmentTreeStore(
		(state) => getOrCreate(state, stateId).childrenByParentId,
	);

export const useDepartmentTreeLoadingIds = (stateId?: DepartmentTreeId) =>
	useDepartmentTreeStore((state) => getOrCreate(state, stateId).loadingIds);

export const collapseAllDepartments = (stateId?: DepartmentTreeId) =>
	useDepartmentTreeStore.setState((states) => ({
		[resolveStateId(stateId)]: {
			...getOrCreate(states, stateId),
			expandedIds: [],
		},
	}));

export const expandLoadedDepartments = (stateId?: DepartmentTreeId) =>
	useDepartmentTreeStore.setState((states) => {
		const currentState = getOrCreate(states, stateId);
		const loadedParentIds = Object.entries(currentState.childrenByParentId)
			.filter(([, children]) => children.length > 0)
			.map(([parentId]) => parentId);

		return {
			[resolveStateId(stateId)]: {
				...currentState,
				expandedIds: loadedParentIds,
			},
		};
	});

export const useNextPageByParentId = (stateId?: DepartmentTreeId) =>
	useDepartmentTreeStore(
		(state) => getOrCreate(state, stateId).nextPageByParentId,
	);

export const loadNextDepartmentChildrenPage = async (
	parentId: DepartmentId,
	stateId?: DepartmentTreeId,
) => {
	const storeId = resolveStateId(stateId);

	const currentState = getOrCreate(useDepartmentTreeStore.getState(), stateId);

	const hasLoaded = Object.prototype.hasOwnProperty.call(
		currentState.childrenByParentId,
		parentId,
	);

	const isLoading = currentState.loadingIds.includes(parentId);

	const page = hasLoaded ? currentState.nextPageByParentId[parentId] : 1;

	if (page == null || isLoading) {
		return;
	}

	useDepartmentTreeStore.setState((states) => {
		const current = getOrCreate(states, stateId);

		return {
			...states,
			[storeId]: {
				...current,
				loadingIds: [...current.loadingIds, parentId],
			},
		};
	});

	try {
		const response = await departmentsApi.getDepartmentChildren({
			parentId,
			page,
			pageSize: 20,
		});

		const result = response.result;

		if (!result) return;

		useDepartmentTreeStore.setState((states) => {
			const current = getOrCreate(states, stateId);

			const existingChildren = current.childrenByParentId[parentId] ?? [];

			const children = Array.from(
				new Map(
					[...existingChildren, ...result.items].map((child) => [
						child.id,
						child,
					]),
				).values(),
			);

			const nextPage = result.page < result.totalPages ? result.page + 1 : null;

			return {
				...states,
				[storeId]: {
					...current,

					childrenByParentId: {
						...current.childrenByParentId,
						[parentId]: children,
					},

					nextPageByParentId: {
						...current.nextPageByParentId,
						[parentId]: nextPage,
					},
				},
			};
		});
	} finally {
		useDepartmentTreeStore.setState((states) => {
			const current = getOrCreate(states, stateId);

			return {
				...states,
				[storeId]: {
					...current,
					loadingIds: current.loadingIds.filter((id) => id !== parentId),
				},
			};
		});
	}
};
