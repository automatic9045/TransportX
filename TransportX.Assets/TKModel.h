#pragma once

#include <CommonStates.h>
#include <d3d11.h>
#include <Effects.h>
#include <Model.h>

using namespace DirectX;

namespace TransportX
{
	namespace Assets
	{
		class TKModel
		{
		private:
			std::unique_ptr<CommonStates> states;
			std::unique_ptr<EffectFactory> fxFactory;
			std::unique_ptr<Model> model;

		public:
			TKModel(ID3D11Device* device, const wchar_t* filePath);
			~TKModel();

			void Draw(ID3D11DeviceContext* context, XMMATRIX world, CXMMATRIX view, CXMMATRIX projection);
		};
	}
}
