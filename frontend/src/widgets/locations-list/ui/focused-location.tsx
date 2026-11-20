import { useLocation, type LocationDto } from "@/entities/locations";
import { LocationCard } from "./location-card";
import { FocusedCard } from "@/shared/ui/focused-card";
import { ListEmpty } from "@/shared/ui/list-empty";

type Props = {
	id: string;
	onEdit: (location: LocationDto) => void;
	onRemove: () => void;
};

export function FocusedLocation({ id, onEdit, onRemove }: Props) {
	const { location, isLoading, isError, error, refetch } = useLocation(id);

	return (
		<FocusedCard
			isLoading={isLoading}
			isError={isError}
			error={error}
			refetch={refetch}
			onRemove={onRemove}
		>
			{location ? (
				<LocationCard location={location} onEdit={() => onEdit(location)} />
			) : (
				<ListEmpty title="Локация не найдена" />
			)}
		</FocusedCard>
	);
}
