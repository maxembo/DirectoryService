import type { InfinityScrollRequest, PaginationRequest } from "@/shared/api";
import type { SortDirectionFilter } from "@/shared/model";

export interface GetDepartmentsRequest extends PaginationRequest {
	locationIds?: string[];
	search?: string;
	isActive?: boolean;
	isArchived?: boolean;
	isParent?: boolean;
	parentId?: string;
	sortBy?: string;
	sortDirection?: SortDirectionFilter;
}

export interface GetDepartmentsInfinityRequest extends InfinityScrollRequest {
	locationIds?: string[];
	search?: string;
	isActive?: boolean;
	isArchived?: boolean;
	isParent?: boolean;
	parentId?: string;
	sortBy?: string;
	sortDirection?: SortDirectionFilter;
}

export interface GetDepartmentTreeRootsRequest extends PaginationRequest {
	prefetch?: number;
	onlyActive?: boolean;
}

export interface GetDepartmentChildrenRequest extends PaginationRequest {
	parentId: string;
	onlyActive?: boolean;
}

export interface ChangeDepartmentActivityRequest {
	departmentId: string;
	isActive: boolean;
}

export interface MoveDepartmentRequest {
	departmentId: string;
	parentId: string | null;
}
