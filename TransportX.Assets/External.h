#pragma once
#include <d3d11.h>
#include <WICTextureLoader.h>
#include "TKModel.h"

extern "C"
{
	__declspec(dllexport) HRESULT __stdcall CreateWICTextureFromFile_(
		ID3D11Device* d3dDevice, ID3D11DeviceContext* d3dContext,
		const wchar_t* szFileName, ID3D11Resource** texture, ID3D11ShaderResourceView** textureView, size_t maxsize);
}
