#include "External.h"

using namespace Bus::Assets;

HRESULT __stdcall CreateWICTextureFromFile_(
	ID3D11Device* d3dDevice, ID3D11DeviceContext* d3dContext,
	const wchar_t* szFileName, ID3D11Resource** texture, ID3D11ShaderResourceView** textureView, size_t maxsize)
{
	HRESULT result = CreateWICTextureFromFile(d3dDevice, d3dContext, szFileName, texture, textureView, maxsize);
	return result;
}
