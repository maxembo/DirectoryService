export type DepartmentId = string;

export type DepartmentShortDto = {
	id: DepartmentId;
	name: string;
	identifier: string;
	path: string;
	isActive: boolean;
	createdAt: string;
	updatedAt: string;
	deletedAt: string | null;
};

export type DepartmentTreeDto = {
	id: DepartmentId;
	name: string;
	identifier: string;
	parentId: DepartmentId | null;
	isActive: boolean;
	depth: number;
	hasChildren: boolean;
};

export type DepartmentSortByFilter = "name" | "path" | "created";

export type DepartmentParentFilter = "all" | "parent" | "children";
