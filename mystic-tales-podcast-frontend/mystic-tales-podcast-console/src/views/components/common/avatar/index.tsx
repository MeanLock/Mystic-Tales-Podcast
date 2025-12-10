import { loginRequiredAxiosInstance } from '@/core/api/rest-api/config/instances/v2'
import { getPublicSource } from '@/core/services/file/file.service'
import React, { type FC, useEffect, useState } from 'react'
import styled from 'styled-components'
import NotFound from '../../../../assets/images/notfound.png'
interface AvatarInputProps {
  src?: string
  fileKey?: string
  size?: number
}

interface InputProps {
  size?: number
}

const Input = styled.div<InputProps>`
  position: relative;
  align-self: center;
  img {
    width: ${p => (p.size ? `${p.size}px` : '186px')};
    height: ${p => (p.size ? `${p.size}px` : '186px')};
    object-fit: cover;
    border-radius: 50%;
  }
  .circle {
    width: ${p => (p.size ? `${p.size}px` : '186px')};
    height: ${p => (p.size ? `${p.size}px` : '186px')};
    border-radius: 50%;
  }
`

const isAbsoluteUrl = (v?: string) => !!v && /^(https?:)?\/\//i.test(v)

const AvatarInput: FC<AvatarInputProps> = ({ src, size, fileKey }) => {
  const [url, setUrl] = useState<string>(src || '')

  useEffect(() => {
    let mounted = true
    ;(async () => {
      if (!fileKey) {
        if (mounted) setUrl(src || '')
        return
      }
      // nếu fileKey đã là URL tuyệt đối thì dùng luôn
      if (isAbsoluteUrl(fileKey)) {
        if (mounted) setUrl(fileKey)
        return
      }
      try {
        const res = await getPublicSource(loginRequiredAxiosInstance, fileKey)
        if (mounted) setUrl(res.success ? res.data.FileUrl : src || '')
      } catch {
        if (mounted) setUrl(src || '')
      }
    })()
    return () => {
      mounted = false
    }
  }, [fileKey, src])

  return (
    <Input size={size}>
      <img src={url || NotFound} alt="Avatar" onError={() => setUrl(NotFound)} />
    </Input>
  )
}

export default AvatarInput
export type { AvatarInputProps }