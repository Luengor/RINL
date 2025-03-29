import { useQuery } from "@tanstack/react-query";
import { useClient } from "./useClient";
import { getMeUserMeGet } from "../client";

export function useUser() {
    // Get the client
    const { client } = useClient();
    
    // Get user data
    const { data: user, status } = useQuery({
        queryKey: ['user-data'],
        queryFn: async () => {
            const req = await getMeUserMeGet({client: client});
            return req.data;
        },

        meta: { errorMessage: 'Error al cargar los datos' },
        staleTime: 1000 * 60 * 5,
    })
    
    const verified = status === 'success' ? user.verified : false;
    
    return {
        verified,
        status,
        user
    }
}