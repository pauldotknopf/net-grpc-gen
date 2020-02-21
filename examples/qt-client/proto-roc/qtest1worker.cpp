#include "qtest1worker.h"
#include "gen.pb.h"
#include "gen.grpc.pb.h"
#include "roc-lib/qrocobjectadapter.h"
using namespace Tests;
class Tests::QTest1WorkerPrivate
{
public:
	QTest1WorkerPrivate() : objectId(0) {}
	google::protobuf::uint64 objectId;
	std::unique_ptr<Tests::Test1ObjectService::Stub> service;
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
		Tests::Test1CreateResponse createResponse;
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
QTest1Worker::QTest1Worker() : QObject(nullptr), d_priv(new QTest1WorkerPrivate())
{
	auto channel = QRocObjectAdapter::getSharedChannel();
	if(channel == nullptr) { qWarning("Set the channel to use via QRocObjectAdapter::setSharedChannel(...)"); return; }
	d_priv->service = Tests::Test1ObjectService::NewStub(channel);
	d_priv->createObject();
}
QTest1Worker::~QTest1Worker()
{
	d_priv->releaseObject();
}
void QTest1Worker::testMethodPrimitive(int val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::Test1TestMethodPrimitiveMethodRequest request;
		Tests::Test1TestMethodPrimitiveMethodResponse response;
		request.set_objectid(d_priv->objectId);
		request.set_value(val);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestMethodPrimitive(&context, request, &response);
		auto responseValue = response.value();
		if(!invokeResult.ok()) {
			emit testMethodPrimitiveDone(responseValue, requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testMethodPrimitiveDone(responseValue, requestId, QString());
		}
	});
}
void QTest1Worker::testMethodNoRequest(int requestId)
{
	QMetaObject::invokeMethod(this, [this, requestId] {
		Tests::Test1TestMethodNoRequestMethodRequest request;
		Tests::Test1TestMethodNoRequestMethodResponse response;
		request.set_objectid(d_priv->objectId);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestMethodNoRequest(&context, request, &response);
		auto responseValue = response.value();
		if(!invokeResult.ok()) {
			emit testMethodNoRequestDone(responseValue, requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testMethodNoRequestDone(responseValue, requestId, QString());
		}
	});
}
