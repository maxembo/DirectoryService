import type { DepartmentId } from "@/entities/departments";
import type { PositionSortByFilter } from "@/entities/positions";
import { ActiveFilter, SortDirectionFilter } from "@/shared/model/filter-types";
import { create } from "zustand";
import { createJSONStorage, persist } from "zustand/middleware";

export type PositionListId = string;

type PositionListState = {
	departmentIds: DepartmentId[];
	search: string;
	isActive: ActiveFilter;
	sortBy: PositionSortByFilter;
	sortDirection: SortDirectionFilter;
};

type PositionListStates = Record<PositionListId, PositionListState | undefined>;

const initialState: PositionListState = {
	departmentIds: [],
	search: "",
	isActive: "all",
	sortBy: "name",
	sortDirection: "asc",
};

const initialStates: PositionListStates = {};

const DEFAULT_STATE_ID = "__default__";

const resolveStateId = (stateId?: PositionListId) =>
	stateId ?? DEFAULT_STATE_ID;

const getOrCreate = (states: PositionListStates, stateId?: PositionListId) => {
	const id = resolveStateId(stateId);

	if (!states[id]) states[id] = { ...initialState };

	return states[id];
};

const usePositionListStore = create<PositionListStates>()(
	persist(() => ({ ...initialStates }), {
		name: "position-list-storage",
		storage: createJSONStorage(() => localStorage),
		partialize: (state) =>
			Object.fromEntries(
				Object.entries(state).filter(([key]) => key === DEFAULT_STATE_ID),
			),
	}),
);

export const usePositionSelectedDepartments = (stateId?: PositionListId) =>
	usePositionListStore((states) => getOrCreate(states, stateId).departmentIds);

export const setPositionSelectedDepartments = (
	departmentIds: DepartmentId[],
	stateId?: PositionListId,
) =>
	usePositionListStore.setState((states) => ({
		[resolveStateId(stateId)]: {
			...getOrCreate(states, stateId),
			departmentIds,
		},
	}));

export const usePositionSearch = (stateId?: PositionListId) =>
	usePositionListStore((states) => getOrCreate(states, stateId).search);

export const setPositionSearch = (search: string, stateId?: PositionListId) =>
	usePositionListStore.setState((states) => ({
		[resolveStateId(stateId)]: {
			...getOrCreate(states, stateId),
			search,
		},
	}));

export const usePositionIsActive = (stateId?: PositionListId) =>
	usePositionListStore((states) => getOrCreate(states, stateId).isActive);

export const setPositionIsActive = (
	isActive: ActiveFilter,
	stateId?: PositionListId,
) =>
	usePositionListStore.setState((states) => ({
		[resolveStateId(stateId)]: {
			...getOrCreate(states, stateId),
			isActive,
		},
	}));

export const usePositionSortBy = (stateId?: PositionListId) =>
	usePositionListStore((states) => getOrCreate(states, stateId).sortBy);

export const setPositionSortBy = (
	sortBy: PositionSortByFilter,
	stateId?: PositionListId,
) =>
	usePositionListStore.setState((states) => ({
		[resolveStateId(stateId)]: {
			...getOrCreate(states, stateId),
			sortBy,
		},
	}));

export const usePositionSortDirection = (stateId?: PositionListId) =>
	usePositionListStore((states) => getOrCreate(states, stateId).sortDirection);

export const setPositionSortDirection = (
	sortDirection: SortDirectionFilter,
	stateId?: PositionListId,
) =>
	usePositionListStore.setState((states) => ({
		[resolveStateId(stateId)]: {
			...getOrCreate(states, stateId),
			sortDirection,
		},
	}));
