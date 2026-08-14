export { departmentsApi } from "./api/api";

export type {
	GetDepartmentChildrenRequest,
	GetDepartmentsInfinityRequest,
	GetDepartmentsRequest,
	GetDepartmentTreeRootsRequest,
} from "./api/types";

export type {
	DepartmentId,
	DepartmentParentFilter,
	DepartmentShortDto,
	DepartmentSortByFilter,
	DepartmentTreeDto,
} from "./model/types";

export type { DepartmentListId } from "./model/department-list-store";

export {
	setDepartmentIsActive,
	setDepartmentIsParent,
	setDepartmentParentId,
	setDepartmentSearch,
	setDepartmentSortBy,
	setDepartmentSortDirection,
	useDepartmentIsActive,
	useDepartmentIsParent,
	useDepartmentParentId,
	useDepartmentSearch,
	useDepartmentSortBy,
	useDepartmentSortDirection,
} from "./model/department-list-store";

export { useInfiniteDepartmentsList } from "./model/use-infinite-departments-list";
