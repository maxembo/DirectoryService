import type { DepartmentDetailDto } from "@/entities/departments";
import {
	Card,
	CardContent,
	CardHeader,
	CardTitle,
} from "@/shared/components/ui/card";
import { CircleCheckBig, CircleX, Clock3 } from "lucide-react";
import { Separator } from "@/shared/components/ui/separator";
import { Badge } from "@/shared/components/ui/badge";
import { ToggleDepartmentActivity } from "@/features/toggle-department-activity";
import { MoveDepartmentAction } from "../move-department/move-department-action";
import { formatDepartmentPath } from "../../lib/format-department-path";

type Props = {
	department: DepartmentDetailDto;
	onSelect: () => void;
};

export function DepartmentDetailCard({ department, onSelect }: Props) {
	return (
		<Card onClick={onSelect}>
			<CardHeader className="space-y-3">
				<div className="grid gap-4">
					<CardTitle className="flex items-center justify-between truncate text-lg leading-tight">
						{department.name}
						<div onClick={(event) => event.stopPropagation()}></div>
						<MoveDepartmentAction department={department} />
					</CardTitle>

					<div>
						<Badge
							variant={department.isActive ? "default" : "secondary"}
							className="shrink-0 whitespace-nowrap"
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
				</div>
			</CardHeader>

			<CardContent className="flex flex-col gap-3">
				<Separator />
				<span className="text-muted-foreground font-mono">
					Идентификатор: {department.identifier}
				</span>

				<Separator />
				<span className="text-muted-foreground font-mono">
					Путь: {formatDepartmentPath(department.path)}
				</span>

				<Separator />
				<div className="text-muted-foreground flex min-w-0 items-center gap-3 text-sm">
					<Clock3 className="h-4 w-4 shrink-0" />
					<span className="min-w-0 truncate">
						Дата создания: {new Date(department.createdAt).toLocaleDateString()}
					</span>
				</div>

				<div className="text-muted-foreground flex min-w-0 items-center gap-3 text-sm">
					<Clock3 className="h-4 w-4 shrink-0" />
					<span className="min-w-0 truncate">
						Дата обновления:{" "}
						{new Date(department.updatedAt).toLocaleDateString()}
					</span>
				</div>
			</CardContent>
			<div onClick={(event) => event.stopPropagation()}>
				<ToggleDepartmentActivity department={department} />
			</div>
		</Card>
	);
}
