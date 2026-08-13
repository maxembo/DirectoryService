import type { LocationDto } from "@/entities/locations";
import { DeleteLocationButton } from "@/features/delete-location";
import { RestoreLocationDialog } from "@/features/restore-location";
import { Badge } from "@/shared/components/ui/badge";
import { Button } from "@/shared/components/ui/button";
import {
	Card,
	CardContent,
	CardHeader,
	CardTitle,
} from "@/shared/components/ui/card";
import { Separator } from "@/shared/components/ui/separator";
import {
	CircleCheckBig,
	CircleX,
	Clock3,
	Globe2,
	MapPin,
	Pencil,
} from "lucide-react";

type Props = {
	location: LocationDto;
	onEdit: () => void;
};

export function LocationCard({ location, onEdit }: Props) {
	const handleEdit = (e: React.MouseEvent) => {
		e.preventDefault();
		e.stopPropagation();

		onEdit();
	};

	return (
		<Card className="min-w-0 transition-shadow hover:shadow-md">
			<CardHeader className="space-y-3">
				<div className="grid grid-cols-[minmax(0,1fr)_auto] items-start gap-4">
					<CardTitle className="min-w-0 flex-1 truncate text-lg leading-tight">
						{location.name}
					</CardTitle>
					{location.isActive ? (
						<div className="flex shrink-0 gap-3">
							<Button
								type="button"
								variant="link"
								size="icon"
								className="h-8 w-8 hover:bg-blue-500"
								onClick={handleEdit}
							>
								<Pencil className="h-4 w-4" />
							</Button>

							<DeleteLocationButton locationId={location.id} />
						</div>
					) : (
						<RestoreLocationDialog
							locationId={location.id}
							name={location.name}
						/>
					)}
				</div>
				<div className="flex items-start justify-between gap-4">
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
				<div className="text-muted-foreground flex items-center gap-2 text-sm">
					<Globe2 className="h-4 w-4 shrink-0" />
					<span>{location.timezone}</span>
				</div>

				<div className="bg-muted/30 space-y-3 rounded-lg border p-3">
					<div className="flex items-start gap-2 text-sm">
						<MapPin className="text-muted-foreground mt-0.5 h-4 w-4 shrink-0" />
						<div className="space-y-1">
							<p className="font-medium">Адрес</p>
							<ul className="text-muted-foreground space-y-1">
								<li>Страна: {location.address.country}</li>
								<li>Город: {location.address.city}</li>
								<li>Улица: {location.address.street}</li>
								<li>Дом: {location.address.house}</li>
							</ul>
						</div>
					</div>

					<Separator />

					<div className="text-muted-foreground flex items-center gap-2 text-xs">
						<Clock3 className="h-3.5 w-3.5" />
						<span>ID: {location.id}</span>
					</div>
				</div>

				<div className="text-muted-foreground flex items-center gap-2 text-sm">
					<Clock3 className="h-4 w-4 shrink-0" />
					{location.isActive ? (
						<span>
							Дата создания: {new Date(location.createdAt).toLocaleDateString()}
						</span>
					) : (
						<span>
							Дата удаления:{" "}
							{location.deletedAt
								? new Date(location.deletedAt).toLocaleDateString("ru-RU")
								: "не указана"}
						</span>
					)}
				</div>

				<Separator />

				<div className="text-muted-foreground flex items-center gap-2 text-sm">
					<Clock3 className="h-4 w-4 shrink-0" />
					<span>
						Дата обновления: {new Date(location.updatedAt).toLocaleDateString()}
					</span>
				</div>
			</CardContent>
		</Card>
	);
}
