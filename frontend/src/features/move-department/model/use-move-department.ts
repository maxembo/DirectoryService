import { departmentsApi } from "@/entities/departments";
import { EnvelopeError, queryClient } from "@/shared/api";
import { useMutation } from "@tanstack/react-query";
import { toast } from "sonner";

export function useMoveDepartment() {
	const mutation = useMutation({
		mutationFn: departmentsApi.moveDepartment,
		onSuccess: async () => {
			await queryClient.invalidateQueries({
				queryKey: [departmentsApi.baseKey],
			});

			toast.success("Перенос подразделения успешно");
		},
		onError: (error) => {
			if (!(error instanceof EnvelopeError)) {
				toast.error("Ошибка при переносе подразделения");
			}
		},
	});

	return {
		moveDepartment: mutation.mutate,
		isPending: mutation.isPending,
		error: mutation.error instanceof EnvelopeError ? mutation.error : undefined,
		reset: mutation.reset,
	};
}
