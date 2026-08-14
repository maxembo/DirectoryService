import type { DepartmentId, DepartmentShortDto } from "@/entities/departments";
import { Badge } from "@/shared/components/ui/badge";

type Props = {
	selectedDepartments: DepartmentShortDto[];
	onRemove: (id: DepartmentId) => void;
};

export function SelectedDepartment({ selectedDepartments, onRemove }: Props) {
	return (
		<div>
			{selectedDepartments.length > 0 && (
				<div className="flex flex-wrap gap-2">
					{selectedDepartments.map((department) => (
						<Badge
							key={department.id}
							variant="secondary"
							className="cursor-pointer"
							onClick={() => onRemove(department.id)}
						>
							{department.name} ×
						</Badge>
					))}
				</div>
			)}
		</div>
	);
}
