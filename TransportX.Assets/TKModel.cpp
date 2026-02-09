#include <filesystem>
#include <iostream>
#include "TKModel.h"

using namespace std::filesystem;

Bus::Assets::TKModel::TKModel(ID3D11Device* device, const wchar_t* filePath)
{
	try
	{
		states = std::make_unique<CommonStates>(device);
		fxFactory = std::make_unique<EffectFactory>(device);
		path directory = path(filePath).parent_path();
		fxFactory->SetDirectory(directory.c_str());
		model = Model::CreateFromCMO(device, filePath, *fxFactory);
	}
	catch (std::exception ex)
	{
		const char* message = ex.what();
		std::cerr << message << std::endl;
		throw;
	}
}

Bus::Assets::TKModel::~TKModel()
{
}

void Bus::Assets::TKModel::Draw(ID3D11DeviceContext* context, XMMATRIX world, CXMMATRIX view, CXMMATRIX projection)
{
	model->Draw(context, *states, world, view, projection);
}
