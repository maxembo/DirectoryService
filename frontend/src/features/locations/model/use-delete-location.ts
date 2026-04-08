import { locationsApi } from "@/entities/locations/api/locations-api";
import { EnvelopeError } from "@/shared/api/errors";
import { queryClient } from "@/shared/api/query-client";
import { useMutation } from "@tanstack/react-query";
import { toast } from "sonner";

export function useDeleteLocation() {
	const mutation = useMutation({
		mutationFn: locationsApi.deleteLocation,
		onSuccess: () => {
			queryClient.invalidateQueries({ queryKey: [locationsApi.baseKey] });
			toast.success(`Локация успешно удалена`);
		},
	});

	return {
		deleteLocation: mutation.mutateAsync,
		isPending: mutation.isPending,
		isError: mutation.isError,
		error: mutation.error instanceof EnvelopeError ? mutation.error : undefined,
		reset: mutation.reset,
	};
}
