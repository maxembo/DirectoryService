import { DepartmentTreeRootsList } from "@/features/department-tree";
import { DepartmentPositionsList } from "./department-positions-list";

export function DepartmentPositions() {
	return (
		<div className="grid h-full min-h-0 grid-cols-2 gap-6">
			<DepartmentTreeRootsList />
			<DepartmentPositionsList />
		</div>
	);
}
