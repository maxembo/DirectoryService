import { Badge } from "@/shared/components/ui/badge";
import type { DepartmentId, DepartmentShortDto } from "@/entities/departments";
import { X } from "lucide-react";

type Props = {
	selectedDepartments: DepartmentShortDto[];
	onRemove: (id: DepartmentId) => void;
};

export function SelectedDepartment({ selectedDepartments, onRemove }: Props) {
	if (selectedDepartments.length === 0) return null;

	return (
		<div className="flex flex-wrap gap-2">
			{selectedDepartments.map((department) => (
				<Badge key={department.id} variant="secondary">
					{department.name}
					<button
						type="button"
						className="text-muted-foreground hover:bg-destructive/10 hover:text-destructive focus-visible:ring-ring inline-flex size-5 items-center justify-center rounded-full transition-colors duration-150 focus-visible:ring-2 focus-visible:outline-none"
						onClick={() => onRemove(department.id)}
						aria-label={`Убрать подразделение ${department.name}`}
					>
						<X className="size-3" aria-hidden="true" />
					</button>
				</Badge>
			))}
		</div>
	);
}
