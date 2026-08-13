import type { PositionDto } from "@/entities/positions";
import { Badge } from "@/shared/components/ui/badge";
import {
	Card,
	CardContent,
	CardHeader,
	CardTitle,
} from "@/shared/components/ui/card";
import { CircleCheckBig, CircleX, Clock3 } from "lucide-react";

type Props = {
	position: PositionDto;
};

export function DepartmentPositionCard({ position }: Props) {
	return (
		<Card className="min-w-0 transition-shadow hover:shadow-md">
			<CardHeader>
				<div className="grid min-w-0 grid-cols-[minmax(0,1fr)_auto] items-center gap-3">
					<div className="flex min-w-0 items-center gap-3">
						<div className="h-5 w-5 shrink-0" aria-hidden="true" />
						<CardTitle className="min-w-0 truncate text-lg leading-tight">
							{position.name}
						</CardTitle>
					</div>

					<Badge
						variant={position.isActive ? "default" : "secondary"}
						className="shrink-0 gap-1 whitespace-nowrap"
					>
						{position.isActive ? (
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
			<CardContent>
				<div className="text-muted-foreground flex min-w-0 items-center gap-3 text-sm">
					<Clock3 className="h-4 w-4 shrink-0" />
					<span className="min-w-0 truncate">
						Дата создания: {new Date(position.createdAt).toLocaleDateString()}
					</span>
				</div>
			</CardContent>
		</Card>
	);
}
