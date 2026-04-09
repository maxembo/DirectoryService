import { Location } from "@/entities/locations/model/types";
import { useDeleteLocation } from "@/features/locations/model/use-delete-location";
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
	Trash2,
} from "lucide-react";

type Props = {
	location: Location;
	onEdit: () => void;
	onDelete: () => void;
};

export function LocationCard({ location, onEdit }: Props) {
	const { deleteLocation, isPending } = useDeleteLocation();

	const handleEdit = (e: React.MouseEvent) => {
		e.preventDefault();
		e.stopPropagation();

		onEdit();
	};

	const handleDelete = async (e: React.MouseEvent) => {
		e.preventDefault();
		e.stopPropagation();

		try {
			await deleteLocation(location.id);
		} catch {}
	};
	return (
		<Card className="transition-shadow hover:shadow-md">
			<CardHeader className="space-y-3">
				<div className="flex justify-between gap-4">
					<CardTitle className="text-lg leading-tight">
						{location.name}
					</CardTitle>

					<div className="flex gap-3">
						<Button
							type="button"
							variant="link"
							size="icon"
							className="ml-auto h-8 w-8 hover:bg-blue-500"
							onClick={handleEdit}
						>
							<Pencil className="h-4 w-4" />
						</Button>

						<Button
							type="button"
							variant="link"
							size="icon"
							className="ml-auto h-8 w-8 hover:bg-red-500"
							onClick={handleDelete}
							disabled={isPending}
						>
							<Trash2 className="h-4 w-4" />
						</Button>
					</div>
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
