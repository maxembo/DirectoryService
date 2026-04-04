import { locationsApi } from "@/entities/locations/api";
import { useMutation } from "@tanstack/react-query";
import { toast } from "sonner";
import { queryClient } from "../../../shared/api/query-client";

export type CreateLocationParams = {
	name: string;
	address: {
		country: string;
		city: string;
		street: string;
		house: string;
	};
	timezone: string;
};

export function useCreateLocation() {
	const mutation = useMutation({
		mutationFn: (params: CreateLocationParams) =>
			locationsApi.createLocation(params),
		onSettled: () => {
			queryClient.invalidateQueries({ queryKey: [locationsApi.baseKey] });
		},
		onSuccess: () => {
			toast.success("Локация успешно создана");
		},
		onError: () => {
			toast.error("Ошибка при создании локации");
		},
	});

	return {
		createLocation: mutation.mutate,
		isPending: mutation.isPending,
		error: mutation.error,
	};
}
