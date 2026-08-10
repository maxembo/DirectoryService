import { DepartmentShortDto } from "@/entities/departments/model/types";
import { RestoreDepartmentDialog } from "@/features/departments/restore-department/ui/restore-department-dialog";
import { Badge } from "@/shared/components/ui/badge";
import {
	Card,
	CardContent,
	CardHeader,
	CardTitle,
} from "@/shared/components/ui/card";
import { Separator } from "@/shared/components/ui/separator";
import { CircleCheckBig, CircleX, Clock3 } from "lucide-react";

type Props = {
	department: DepartmentShortDto;
};

export function ArchivedDepartmentCard({ department }: Props) {
	return (
		<Card className="min-w-0 transition-shadow hover:shadow-md">
			<CardHeader className="space-y-3">
				<div className="grid grid-cols-[minmax(0,1fr)_auto] items-start gap-4">
					<CardTitle className="min-w-0 flex-1 truncate text-lg leading-tight">
						{department.name}
					</CardTitle>

					{!department.isActive && (
						<RestoreDepartmentDialog
							departmentId={department.id}
							name={department.name}
						/>
					)}
				</div>

				<div className="flex items-start justify-between gap-4">
					<Badge
						variant={department.isActive ? "default" : "secondary"}
						className="shrink-0 gap-1 whitespace-nowrap"
					>
						{department.isActive ? (
							<>
								<CircleCheckBig className="h-3.5 w-3.5" />
								Активна
							</>
						) : (
							<>
								<CircleX className="h-3.5 w-3.5" />
								Неактивна
							</>
						)}
					</Badge>
				</div>
			</CardHeader>

			<CardContent className="flex flex-col gap-3">
				<Separator />
				<span className="text-muted-foreground font-mono">
					Идентификатор: {department.identifier}
				</span>

				<Separator />
				<span className="text-muted-foreground font-mono">
					Путь: {department.path}
				</span>

				<Separator />
				<div className="flex min-w-0 items-center gap-3 text-sm text-muted-foreground">
					<Clock3 className="h-4 w-4 shrink-0" />
					<span className="min-w-0 truncate">
						Дата удаления:{" "}
						{department.deletedAt
							? new Date(department.deletedAt).toLocaleDateString()
							: "Не указана"}
					</span>
				</div>
			</CardContent>
		</Card>
	);
}
