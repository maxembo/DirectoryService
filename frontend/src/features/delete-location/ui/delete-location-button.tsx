import { Button } from "@/shared/components/ui/button";
import { Trash2 } from "lucide-react";
import { useDeleteLocation } from "../model/use-delete-location";

type Props = {
	locationId: string;
};

export function DeleteLocationButton({ locationId }: Props) {
	const { deleteLocation, isPending } = useDeleteLocation();

	const handleDelete = async (event: React.MouseEvent<HTMLButtonElement>) => {
		event.preventDefault();
		event.stopPropagation();

		await deleteLocation(locationId);
	};

	return (
		<Button
			type="button"
			variant="link"
			size="icon"
			className="h-8 w-8 hover:bg-red-500"
			onClick={handleDelete}
			disabled={isPending}
		>
			<Trash2 className="h-4 w-4" />
		</Button>
	);
}
