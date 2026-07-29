import { DepartmentShortDto } from "@/entities/departments/model/types";
import { Badge } from "@/shared/components/ui/badge";
import {
	Card,
	CardContent,
	CardDescription,
	CardHeader,
	CardTitle,
} from "@/shared/components/ui/card";
import { CircleCheckBig, CircleX, Clock3 } from "lucide-react";

type Props = {
	department: DepartmentShortDto;
};

export function DepartmentCard({ department }: Props) {
	return (
		<Card className="min-w-0 transition-shadow hover:shadow-md">
			<CardHeader>
				<div className="grid min-w-0 grid-cols-[minmax(0,1fr)_auto] items-center gap-3">
					<div className="flex min-w-0 items-center gap-3">
						<div className="h-5 w-5 shrink-0" aria-hidden="true" />
						<CardTitle className="min-w-0 truncate text-lg leading-tight">
							{department.name}
						</CardTitle>
					</div>

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
			<CardContent className="p-6">
				<CardDescription className="flex items-center justify-between">
					<span>{department.identifier}</span>

					<div className="flex items-center gap-2">
						<Clock3 className="h-4 w-4" />
						<span>
							Дата создания:{" "}
							{new Date(department.createdAt).toLocaleDateString()}
						</span>
					</div>
				</CardDescription>
			</CardContent>
		</Card>
	);
}
