export type ActiveFilter = "active" | "inactive" | "all";

export type FilterOption<T extends string> = {
	value: T;
	label: string;
};

export type SortDirectionFilter = "asc" | "desc";
