import { locationsApi } from "@/entities/locations/api";
import { EnvelopeError } from "@/shared/api/errors";
import { useMutation } from "@tanstack/react-query";
import { toast } from "sonner";
import { queryClient } from "../../../shared/api/query-client";

export function useCreateLocation() {
	const mutation = useMutation({
		mutationFn: locationsApi.createLocation,
		onSettled: () => {
			queryClient.invalidateQueries({ queryKey: [locationsApi.baseKey] });
		},
		onSuccess: () => {
			toast.success("Локация успешно создана");
		},
		onError: (error) => {
			if (error instanceof EnvelopeError) {
				toast.error(error.allMessages);
				return;
			}

			toast.error("Ошибка при создании локации");
		},
	});

	return {
		createLocation: mutation.mutate,
		isPending: mutation.isPending,
		isError: mutation.isError,
		error: mutation.error instanceof EnvelopeError ? mutation.error : undefined,
	};
}
