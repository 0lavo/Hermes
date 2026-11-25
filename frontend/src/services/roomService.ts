import { api } from './api'

export type RoomStatus = 'Live' | 'Ready' | 'Ended'

export interface Room {
  id: number
  code: string
  name: string
  subject?: string
  status: RoomStatus
  createdAt: string
}

export interface CreateRoomPayload {
  name: string
  subject?: string
}

export interface JoinRoomResponse {
  anonymous: boolean
}

export async function getMyRooms(): Promise<Room[]> {
  const { data } = await api.get<Room[]>('/sessions')
  return data
}

export async function createRoom(payload: CreateRoomPayload): Promise<Room> {
  const { data } = await api.post<Room>('/sessions', payload)
  return data
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

export async function startRoom(code: string): Promise<void> {
  await api.post(`/sessions/${encodeURIComponent(code)}/start`)
}

export async function endRoom(code: string): Promise<void> {
  await api.post(`/sessions/${encodeURIComponent(code)}/end`)
}