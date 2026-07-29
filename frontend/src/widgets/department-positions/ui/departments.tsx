import { DepartmentTreeRootsList } from "@/features/departments/department-tree/ui/department-tree-roots-list";
import { DepartmentPositionsList } from "./department-positions-list";

export function Departments() {
	return (
		<div className="grid h-full min-h-0 grid-cols-2 gap-6">
			<DepartmentTreeRootsList />

			<DepartmentPositionsList />
		</div>
	);
}
