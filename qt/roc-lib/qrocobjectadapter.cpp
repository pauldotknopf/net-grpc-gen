#include <roc-lib/qrocobjectadapter.h>

std::shared_ptr<grpc::Channel> QRocObjectAdapter::_sharedChannel = nullptr;

QRocObjectAdapter::QRocObjectAdapter(QObject* parent) : QObject(parent)
{

}

QRocObjectAdapter::~QRocObjectAdapter()
{

}

void QRocObjectAdapter::setSharedChannel(std::shared_ptr<grpc::Channel> channel)
{
    _sharedChannel = channel;
}

std::shared_ptr<grpc::Channel> QRocObjectAdapter::getSharedChannel()
{
    return _sharedChannel;
}
