import {
	DepartmentListId,
	setDepartmentSearch,
	useDepartmentSearch,
} from "@/features/departments/model/department-list-store";
import { SearchInput } from "@/shared/components/search-input";

export function SelectDepartmentSearch({
	stateId,
}: {
	stateId?: DepartmentListId;
}) {
	const search = useDepartmentSearch(stateId);

	return (
		<SearchInput
			value={search}
			onChange={(value) => setDepartmentSearch(value, stateId)}
		/>
	);
}
