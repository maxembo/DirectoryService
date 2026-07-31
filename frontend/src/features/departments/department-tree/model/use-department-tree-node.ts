import { DepartmentTreeDto } from "@/entities/departments/model/types";
import {
	DepartmentTreeId,
	loadNextDepartmentChildrenPage,
	setDepartmentTreeSelectedId,
	toggleDepartmentTreeExpandedId,
	useDepartmentTreeChildrenByParentId,
	useDepartmentTreeExpandedIds,
	useDepartmentTreeLoadingIds,
	useDepartmentTreeSelectedId,
	useNextPageByParentId,
} from "./department-tree-store";

type Props = {
	department: DepartmentTreeDto;
	stateId?: DepartmentTreeId;
};

export function useDepartmentTreeNode({ department, stateId }: Props) {
	const selectedId = useDepartmentTreeSelectedId(stateId);
	const expandedIds = useDepartmentTreeExpandedIds(stateId);
	const loadingIds = useDepartmentTreeLoadingIds(stateId);
	const childrenByParentId = useDepartmentTreeChildrenByParentId(stateId);
	const nextPageByParentId = useNextPageByParentId(stateId);

	const children = childrenByParentId[department.id] ?? [];
	const isSelected = selectedId === department.id;
	const isExpanded = expandedIds.includes(department.id);
	const isLoading = loadingIds.includes(department.id);
	const hasChildren = department.hasChildren;

	const nextPage = nextPageByParentId[department.id];

	const canLoadMore = typeof nextPage === "number";

	const handleToggle = () => {
		void toggleDepartmentTreeExpandedId(department.id, hasChildren, stateId);
	};

	const handleSelect = () => {
		setDepartmentTreeSelectedId(department.id, stateId);
	};

	const handleLoadMore = () => {
		loadNextDepartmentChildrenPage(department.id, stateId);
	};
	return {
		children,
		isSelected,
		isExpanded,
		isLoading,
		canLoadMore,
		hasChildren,
		handleToggle,
		handleSelect,
		handleLoadMore,
	};
}
