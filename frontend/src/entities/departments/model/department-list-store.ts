import type { DepartmentParentFilter, DepartmentSortByFilter } from "./types";
import { PAGE_SIZE } from "@/shared/api";
import type { ActiveFilter, SortDirectionFilter } from "@/shared/model";
import { create } from "zustand";
import { createJSONStorage, persist } from "zustand/middleware";

export type DepartmentListId = string;

type DepartmentsListState = {
	selectedLocations: string[];
	search: string;
	parentId: string;
	isParent: DepartmentParentFilter;
	isActive: ActiveFilter;
	sortBy: DepartmentSortByFilter;
	sortDirection: SortDirectionFilter;
	pageSize: number;
};

type DepartmentListStates = Record<
	DepartmentListId,
	DepartmentsListState | undefined
>;

const initialState: DepartmentsListState = {
	selectedLocations: [],
	search: "",
	parentId: "",
	isParent: "all",
	isActive: "all",
	sortBy: "name",
	sortDirection: "asc",
	pageSize: PAGE_SIZE,
};

const DEFAULT_STATE_ID = "__default__";

const initialStates: DepartmentListStates = {};

const resolveStateId = (stateId?: DepartmentListId) =>
	stateId ?? DEFAULT_STATE_ID;

const getOrCreate = (
	states: DepartmentListStates,
	stateId?: DepartmentListId,
) => {
	const id = resolveStateId(stateId);

	if (!states[id]) {
		states[id] = { ...initialState };
	}

	return states[id];
};

const useDepartmentListStore = create<DepartmentListStates>()(
	persist(
		() => ({
			...initialStates,
		}),
		{
			name: "department-list-storage",
			storage: createJSONStorage(() => localStorage),
			partialize: (state) =>
				Object.fromEntries(
					Object.entries(state).filter(([key]) => key === DEFAULT_STATE_ID),
				),
		},
	),
);

export const useDepartmentSelectedLocations = (stateId?: DepartmentListId) =>
	useDepartmentListStore(
		(states) => getOrCreate(states, stateId).selectedLocations,
	);

export const setDepartmentSelectedLocations = (
	selectedLocations: string[],
	stateId?: DepartmentListId,
) =>
	useDepartmentListStore.setState((states) => ({
		[resolveStateId(stateId)]: {
			...getOrCreate(states, stateId),
			selectedLocations,
		},
	}));

export const useDepartmentSearch = (stateId?: DepartmentListId) =>
	useDepartmentListStore((states) => getOrCreate(states, stateId).search);

export const setDepartmentSearch = (
	search: string,
	stateId?: DepartmentListId,
) =>
	useDepartmentListStore.setState((states) => ({
		[resolveStateId(stateId)]: {
			...getOrCreate(states, stateId),
			search,
		},
	}));

export const useDepartmentIsActive = (stateId?: DepartmentListId) =>
	useDepartmentListStore((states) => getOrCreate(states, stateId).isActive);

export const setDepartmentIsActive = (
	isActive: ActiveFilter,
	stateId?: DepartmentListId,
) =>
	useDepartmentListStore.setState((states) => ({
		[resolveStateId(stateId)]: {
			...getOrCreate(states, stateId),
			isActive,
		},
	}));

export const useDepartmentIsParent = (stateId?: DepartmentListId) =>
	useDepartmentListStore((states) => getOrCreate(states, stateId).isParent);

export const setDepartmentIsParent = (
	isParent: DepartmentParentFilter,
	stateId?: DepartmentListId,
) =>
	useDepartmentListStore.setState((states) => ({
		[resolveStateId(stateId)]: {
			...getOrCreate(states, stateId),
			isParent,
		},
	}));

export const useDepartmentParentId = (stateId?: DepartmentListId) =>
	useDepartmentListStore((states) => getOrCreate(states, stateId).parentId);

export const setDepartmentParentId = (
	parentId: string,
	stateId?: DepartmentListId,
) =>
	useDepartmentListStore.setState((states) => ({
		[resolveStateId(stateId)]: {
			...getOrCreate(states, stateId),
			parentId,
		},
	}));

export const useDepartmentSortBy = (stateId?: DepartmentListId) =>
	useDepartmentListStore((states) => getOrCreate(states, stateId).sortBy);

export const setDepartmentSortBy = (
	sortBy: DepartmentSortByFilter,
	stateId?: DepartmentListId,
) =>
	useDepartmentListStore.setState((states) => ({
		[resolveStateId(stateId)]: {
			...getOrCreate(states, stateId),
			sortBy,
		},
	}));

export const useDepartmentSortDirection = (stateId?: DepartmentListId) =>
	useDepartmentListStore(
		(states) => getOrCreate(states, stateId).sortDirection,
	);

export const setDepartmentSortDirection = (
	sortDirection: SortDirectionFilter,
	stateId?: DepartmentListId,
) =>
	useDepartmentListStore.setState((states) => ({
		[resolveStateId(stateId)]: {
			...getOrCreate(states, stateId),
			sortDirection,
		},
	}));
