#include "qtest2worker.h"
#include "gen.pb.h"
#include "gen.grpc.pb.h"
#include "roc-lib/qrocobjectadapter.h"
using namespace Tests;
class Tests::QTest2WorkerPrivate
{
public:
	QTest2WorkerPrivate() : objectId(0) {}
	google::protobuf::uint64 objectId;
	std::unique_ptr<Tests::Test2ObjectService::Stub> service;
	grpc::ClientContext objectRequestContext;
	std::unique_ptr<grpc::ClientReaderWriter<google::protobuf::Any, google::protobuf::Any>> objectRequest;
	
	void createObject()
	{
		objectRequest = service->Create(&objectRequestContext);
		google::protobuf::Any createResponseAny;
		if(!objectRequest->Read(&createResponseAny))
		{
			qCritical("Failed to read request from object creation.");
			objectRequest.release();
			return;
		}
		Tests::Test2CreateResponse createResponse;
		if(!createResponseAny.UnpackTo(&createResponse))
		{
			qCritical("Failed to unpack request from object creation.");
			objectRequest.release();
			return;
		}
		objectId = createResponse.objectid();
	}
	void releaseObject()
	{
		if(objectRequest != nullptr)
		{
			auto result = objectRequest->Finish();
			if(!result.ok()) { qCritical("Couldn't dispose of object: %s", result.error_message().c_str()); }
		}
	}
	bool isValid()
	{
		return objectRequest != nullptr;
	}
};
QTest2Worker::QTest2Worker() : QObject(nullptr), d_priv(new QTest2WorkerPrivate())
{
	auto channel = QRocObjectAdapter::getSharedChannel();
	if(channel == nullptr) { qWarning("Set the channel to use via QRocObjectAdapter::setSharedChannel(...)"); return; }
	d_priv->service = Tests::Test2ObjectService::NewStub(channel);
	d_priv->createObject();
}
QTest2Worker::~QTest2Worker()
{
	d_priv->releaseObject();
}
