export type DepartmentId = string;

export type DepartmentDetailDto = {
	id: DepartmentId;
	name: string;
	identifier: string;
	parentId: DepartmentId | null;
	isActive: boolean;
	path: string;
	createdAt: string;
	updatedAt: string;
	deletedAt: string | null;
};

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
	path: string;
	deletedAt: string | null;
};

export type DepartmentSortByFilter = "name" | "path" | "created";

export type DepartmentParentFilter = "all" | "parent" | "children";
