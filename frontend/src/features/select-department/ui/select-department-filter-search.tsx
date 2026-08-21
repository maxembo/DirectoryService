import { SearchInput } from "@/shared/components/search-input";
import {
	setDepartmentSearch,
	useDepartmentSearch,
	type DepartmentListId,
} from "@/entities/departments";

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
