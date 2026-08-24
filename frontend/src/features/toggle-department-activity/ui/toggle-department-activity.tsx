import type { DepartmentTreeDto } from "@/entities/departments";
import { Label } from "@/shared/components/ui/label";
import { Switch } from "@/shared/components/ui/switch";
import { cn } from "@/shared/lib/utils";
import { LoaderCircle } from "lucide-react";
import { useToggleDepartmentActivity } from "../model/use-toggle-department-activity";

type Props = {
	department: DepartmentTreeDto;
};

export function ToggleDepartmentActivity({ department }: Props) {
	const { toggleDepartmentActivity, isPending } = useToggleDepartmentActivity();
	const switchId = `department-activity-${department.id}`;
	const statusLabel = department.isActive ? "Активно" : "Неактивно";

	return (
		<div className="flex min-w-32 items-center justify-end gap-2">
			<Switch
				id={switchId}
				checked={department.isActive}
				disabled={isPending}
				onCheckedChange={(isActive) =>
					toggleDepartmentActivity({ departmentId: department.id, isActive })
				}
			/>
			<Label
				htmlFor={switchId}
				className={cn(
					"flex w-20 items-center gap-1.5 text-xs font-medium",
					department.isActive ? "text-emerald-700" : "text-muted-foreground",
				)}
				aria-live="polite"
			>
				{isPending ? (
					<LoaderCircle className="size-3.5 animate-spin" aria-hidden="true" />
				) : (
					<span
						className={cn(
							"size-2 rounded-full",
							department.isActive ? "bg-emerald-500" : "bg-muted-foreground/60",
						)}
						aria-hidden="true"
					/>
				)}
				{isPending ? "Сохранение" : statusLabel}
			</Label>
		</div>
	);
}
