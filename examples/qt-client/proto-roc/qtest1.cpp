#include "qtest1.h"
#include <gen.grpc.pb.h>
#include <roc-lib/qrocobjectadapter.h>
#include <QDebug>

using namespace Tests;

class Tests::QTest1Private
{
public:
    QTest1Private() : objectId(0) {

    }
    std::unique_ptr<Tests::Test1ObjectService::Stub> test1ObjectService;
    grpc::ClientContext objectRequestContext;
    std::unique_ptr<grpc::ClientReaderWriter<google::protobuf::Any, google::protobuf::Any>> objectRequest;
    google::protobuf::uint64 objectId;

    void createObject() {
        objectRequest = test1ObjectService->Create(&objectRequestContext);
        google::protobuf::Any createResponseAny;
        if(!objectRequest->Read(&createResponseAny)) {
            qCritical("Failed to read request from object creation.");
            objectRequest.release();
            return;
        }

        Tests::Test1CreateResponse createResponse;
        if(!createResponseAny.UnpackTo(&createResponse)) {
            qCritical("Couldn't unpack request to CreateResponse.");
            objectRequest.release();
            return;
        }

        objectId = createResponse.objectid();
    }

    void releaseObject() {
        if(objectRequest != nullptr) {
            auto result = objectRequest->Finish();
            if(!result.ok()) {
                qCritical("Couldn't dispose of the object: %s", result.error_message().c_str());
            }
        }
    }

    bool isValid() {
        return objectRequest != nullptr;
    }
};

QTest1::QTest1(QObject* parent) : QObject(parent), d_priv(new QTest1Private())
{
    auto channel = QRocObjectAdapter::getSharedChannel();
    if(channel == nullptr) {
        qWarning("Set the channel to use via QRocObjectAdapter::setSharedChannel(...)");
        return;
    }
    d_priv->test1ObjectService = Test1ObjectService::NewStub(channel);
    d_priv->createObject();
}

QTest1::~QTest1()
{
    d_priv->releaseObject();
}
