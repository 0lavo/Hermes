
import { api } from './api'

export interface JoinRoomResponse {
  anonymous: boolean
}

export async function joinRoom(
  code: string,
  anonymous: boolean,
): Promise<JoinRoomResponse> {
  const response = await api.post<JoinRoomResponse>(
    `/sessions/${encodeURIComponent(code)}/join`,
    { anonymous },
  )

  return response.data
}
