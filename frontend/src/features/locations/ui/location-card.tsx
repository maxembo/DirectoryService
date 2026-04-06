import { Badge } from "@/shared/components/ui/badge";
import {
	Card,
	CardContent,
	CardHeader,
	CardTitle,
} from "@/shared/components/ui/card";
import { Separator } from "@/shared/components/ui/separator";
import { CircleCheckBig, CircleX, Clock3, Globe2, MapPin } from "lucide-react";
import { Location } from "../../../entities/locations/types";

type Props = {
	location: Location;
};

export function LocationCard({ location }: Props) {
	return (
		<Card className="transition-shadow hover:shadow-md">
			<CardHeader className="space-y-3">
				<div className="flex items-start justify-between gap-4">
					<CardTitle className="text-lg leading-tight">
						{location.name}
					</CardTitle>

					<Badge
						variant={location.isActive ? "default" : "secondary"}
						className="gap-1 whitespace-nowrap"
					>
						{location.isActive ? (
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

			<CardContent className="space-y-3">
				<div className="flex items-center gap-2 text-sm text-muted-foreground">
					<Globe2 className="h-4 w-4 shrink-0" />
					<span>{location.timezone}</span>
				</div>

				<div className="space-y-3 rounded-lg border bg-muted/30 p-3">
					<div className="flex items-start gap-2 text-sm">
						<MapPin className="mt-0.5 h-4 w-4 shrink-0 text-muted-foreground" />
						<div className="space-y-1">
							<p className="font-medium">Адрес</p>
							<ul className="space-y-1 text-muted-foreground">
								<li>Страна: {location.address.country}</li>
								<li>Город: {location.address.city}</li>
								<li>Улица: {location.address.street}</li>
								<li>Дом: {location.address.house}</li>
							</ul>
						</div>
					</div>

					<Separator />

					<div className="flex items-center gap-2 text-xs text-muted-foreground">
						<Clock3 className="h-3.5 w-3.5" />
						<span>ID: {location.id}</span>
					</div>
				</div>

				<div className="flex items-center gap-2 text-sm text-muted-foreground">
					<Clock3 className="h-4 w-4 shrink-0" />
					<span>
						Дата создания: {new Date(location.createdAt).toLocaleDateString()}
					</span>
				</div>

				<Separator />

				<div className="flex items-center gap-2 text-sm text-muted-foreground">
					<Clock3 className="h-4 w-4 shrink-0" />
					<span>
						Дата обновления: {new Date(location.updatedAt).toLocaleDateString()}
					</span>
				</div>
			</CardContent>
		</Card>
	);
}
